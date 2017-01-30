using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Hosting;
using System.Web.Security;
using MongoMembership.Mongo;
using MongoMembership.Utils;

namespace MongoMembership.Providers
{
    public class MongoMembershipProvider : MembershipProvider
    {
        #region Fields
        private IMongoGateway mongoGateway;
        private bool enablePasswordReset;
        private bool enablePasswordRetrieval;
        private int maxInvalidPasswordAttempts;
        private int minRequiredNonAlphanumericCharacters;
        private int minRequiredPasswordLength;
        private int passwordAttemptWindow;
        private MembershipPasswordFormat passwordFormat;
        private string passwordStrengthRegularExpression;
        private bool requiresQuestionAndAnswer;
        private bool requiresUniqueEmail;
        #endregion

        #region Properties
        internal string MongoConnectionString { get; private set; }

        public override string ApplicationName { get; set; }

        public override bool EnablePasswordReset => enablePasswordReset;

        public override bool EnablePasswordRetrieval => enablePasswordRetrieval;

        public override int MaxInvalidPasswordAttempts => maxInvalidPasswordAttempts;

        public override int MinRequiredNonAlphanumericCharacters => minRequiredNonAlphanumericCharacters;

        public override int MinRequiredPasswordLength => minRequiredPasswordLength;

        public override int PasswordAttemptWindow => passwordAttemptWindow;

        public override MembershipPasswordFormat PasswordFormat => passwordFormat;

        public override string PasswordStrengthRegularExpression => passwordStrengthRegularExpression;

        public override bool RequiresQuestionAndAnswer => requiresQuestionAndAnswer;

        public override bool RequiresUniqueEmail => requiresUniqueEmail;

        #endregion

        #region Public Methods
        public override void Initialize(string name, NameValueCollection config)
        {
            ApplicationName = config["applicationName"].ConvertOrDefault(HostingEnvironment.ApplicationVirtualPath);

            enablePasswordReset = config["enablePasswordReset"].ConvertOrDefault(true);
            enablePasswordRetrieval = config["enablePasswordRetrieval"].ConvertOrDefault(false);
            maxInvalidPasswordAttempts = config["maxInvalidPasswordAttempts"].ConvertOrDefault(5);
            minRequiredNonAlphanumericCharacters = config["minRequiredNonAlphanumericCharacters"].ConvertOrDefault(1);
            minRequiredPasswordLength = config["minRequiredPasswordLength"].ConvertOrDefault(7);
            passwordAttemptWindow = config["passwordAttemptWindow"].ConvertOrDefault(10);
            passwordFormat = config["passwordFormat"].ConvertOrDefault(MembershipPasswordFormat.Hashed);
            passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"].ConvertOrDefault(string.Empty);
            requiresQuestionAndAnswer = config["requiresQuestionAndAnswer"].ConvertOrDefault(false);
            requiresUniqueEmail = config["requiresUniqueEmail"].ConvertOrDefault(true);
            MongoConnectionString = Util.GetConnectionStringByName(config["connectionStringKeys"].ConvertOrDefault(string.Empty));

            mongoGateway = new MongoGateway(MongoConnectionString);

            if (PasswordFormat == MembershipPasswordFormat.Hashed && EnablePasswordRetrieval)
            {
                throw new ProviderException("Configured settings are invalid: Hashed passwords cannot be retrieved. Either set the password format to different type, or set enablePasswordRetrieval to false.");
            }

            base.Initialize(name, config);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (username.IsNullOrWhiteSpace()) return false;

            var user = mongoGateway.GetByUserName(ApplicationName, username);

            if (!IsPasswordCorrect(user, oldPassword))
                return false;

            var validatePasswordEventArgs = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(validatePasswordEventArgs);

            if (validatePasswordEventArgs.Cancel)
                throw new MembershipPasswordException(validatePasswordEventArgs.FailureInformation.Message);

            user.ChangePassword(Encode(newPassword, user.PasswordSalt));
            mongoGateway.UpdateUser(user);
            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (username.IsNullOrWhiteSpace()) return false;
            return UpdateAndReturn(username, user =>
            {
                if (!IsPasswordCorrect(user, password)) return false;
                user.ChangePasswordQuestionAndAnswer(newPasswordQuestion, Encode(newPasswordAnswer, user.PasswordSalt));
                return true;
            });
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (providerUserKey == null)
            {
                providerUserKey = Guid.NewGuid();
            }

            var validatePasswordEventArgs = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(validatePasswordEventArgs);

            if (validatePasswordEventArgs.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresQuestionAndAnswer)
            {
                if (passwordQuestion.IsNullOrWhiteSpace())
                {
                    status = MembershipCreateStatus.InvalidQuestion;
                    return null;
                }
                if (passwordAnswer.IsNullOrWhiteSpace())
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
            }

            if (GetUser(username, false) != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            if (GetUser(providerUserKey, false) != null)
            {
                status = MembershipCreateStatus.DuplicateProviderUserKey;
                return null;
            }

            if (RequiresUniqueEmail && !GetUserNameByEmail(email).IsNullOrWhiteSpace())
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            var buffer = new byte[16];
            buffer.RngGenerator();
            var passwordSalt = buffer.ToBase64String();

            var createDate = DateTime.UtcNow;
            var user = new User
            {
                Id = providerUserKey.ToString(),
                ApplicationName = ApplicationName,
                CreateDate = createDate,
                EmailLowercase = email?.ToLowerInvariant(),
                Email = email,
                FailedPasswordAnswerAttemptCount = 0,
                FailedPasswordAnswerAttemptWindowStart = createDate,
                FailedPasswordAttemptCount = 0,
                FailedPasswordAttemptWindowStart = createDate,
                IsApproved = isApproved,
                IsDeleted = false,
                IsLockedOut = false,
                LastActivityDate = createDate,
                LastLockedOutDate = new DateTime(1970, 1, 1),
                LastLoginDate = createDate,
                LastPasswordChangedDate = createDate,
                Password = Encode(password, passwordSalt),
                PasswordAnswer = Encode(passwordAnswer, passwordSalt),
                PasswordQuestion = passwordQuestion,
                PasswordSalt = passwordSalt,
                UsernameLowercase = username?.ToLowerInvariant(),
                Username = username
            };

            mongoGateway.CreateUser(user);
            status = MembershipCreateStatus.Success;
            return GetUser(username, false);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (username.IsNullOrWhiteSpace()) return false;
            return UpdateAndReturn(username, user =>
            {
                user.Delete();
                return true;
            });
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            var users = mongoGateway.GetAllByEmail(ApplicationName, emailToMatch, pageIndex, pageSize, out totalRecords);

            foreach (var user in users)
            {
                membershipUsers.Add(user.ToMembershipUser(Name));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            var users = mongoGateway.GetAllByUserName(ApplicationName, usernameToMatch, pageIndex, pageSize, out totalRecords);

            foreach (var user in users)
            {
                membershipUsers.Add(user.ToMembershipUser(Name));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            var users = mongoGateway.GetAll(ApplicationName, pageIndex, pageSize, out totalRecords);

            foreach (var user in users)
            {
                membershipUsers.Add(user.ToMembershipUser(Name));
            }

            return membershipUsers;
        }

        public override int GetNumberOfUsersOnline()
        {
            var timeSpan = TimeSpan.FromMinutes(Membership.UserIsOnlineTimeWindow);
            return mongoGateway.GetUserForPeriodOfTime(ApplicationName, timeSpan);
        }

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
                throw new NotSupportedException("This Membership Provider has not been configured to support password retrieval.");

            var user = mongoGateway.GetByUserName(ApplicationName, username);
            ValidatePasswordAnswer(user, answer);

            return DecodePassword(user.Password);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return UpdateAndReturn(username, user =>
            {
                if (user == null) return null;
                if (userIsOnline) user.UpdateLastActivityDate();
                return user.ToMembershipUser(Name);
            });
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var user = mongoGateway.GetById(providerUserKey.ToString());

            if (user == null)
                return null;

            if (userIsOnline)
            {
                user.UpdateLastActivityDate();
                mongoGateway.UpdateUser(user);
            }

            return user.ToMembershipUser(Name);
        }

        public override string GetUserNameByEmail(string email)
        {
            var user = mongoGateway.GetByEmail(ApplicationName, email);
            return user?.Username;
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
                throw new NotSupportedException("This provider is not configured to allow password resets. To enable password reset, set enablePasswordReset to \"true\" in the configuration file.");

            return UpdateAndReturn(username, user =>
            {
                ValidatePasswordAnswer(user, answer);
                var password = Membership.GeneratePassword(MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);
                user.ChangePassword(Encode(password, user.PasswordSalt));
                return password;
            });
        }

        public override bool UnlockUser(string username)
        {
            if (username.IsNullOrWhiteSpace()) return false;
            return UpdateAndReturn(username, user =>
            {
                if (user == null) return false;
                user.Unlock();
                return true;
            });
        }

        public override void UpdateUser(MembershipUser membershipUser)
        {
            var user = mongoGateway.GetById(membershipUser.ProviderUserKey?.ToString());

            if (user == null)
                throw new ProviderException("The membershipUser was not found.");

            user.Update(membershipUser);

            mongoGateway.UpdateUser(user);
        }

        public override bool ValidateUser(string username, string password)
        {
            return UpdateAndReturn(username, user =>
            {
                if (user == null || !user.IsApproved || user.IsLockedOut)
                    return false;

                var isPasswordCorrect = IsPasswordCorrect(user, password);
                if (isPasswordCorrect)
                {
                    user.UpdateLastLoginDate();
                }
                else
                {
                    user.PasswordValidationFailed();
                }
                return isPasswordCorrect;
            });
        }
        #endregion

        #region Private Methods
        private T UpdateAndReturn<T>(string username, Func<User, T> updater)
        {
            var user = mongoGateway.GetByUserName(ApplicationName, username);
            var result = updater(user);
            if (user != null) mongoGateway.UpdateUser(user);
            return result;
        }

        private string DecodePassword(string password)
        {
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    return password;

                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Hashed passwords cannot be decoded.");

                default:
                    var passwordBytes = Convert.FromBase64String(password);
                    var decryptedBytes = DecryptPassword(passwordBytes);
                    return decryptedBytes?.ToUnicodeString(16);
            }
        }

        private string Encode(string password, string salt)
        {
            if (password == null || PasswordFormat == MembershipPasswordFormat.Clear)
            {
                return password;
            }

            var passwordBytes = password.ToByteArray();
            var saltBytes = Convert.FromBase64String(salt);
            var allBytes = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, allBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, allBytes, saltBytes.Length, passwordBytes.Length);

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                return allBytes.ComputeHash().ToBase64String();
            }

            return EncryptPassword(allBytes).ToBase64String();
        }

        private bool IsPasswordCorrect(User user, string password)
        {
            return user.Password == Encode(password, user.PasswordSalt);
        }

        private void ValidatePasswordAnswer(User user, string passwordAnswer)
        {
            if (!RequiresQuestionAndAnswer) return;

            if (user.PasswordAnswer != Encode(passwordAnswer, user.PasswordSalt))
                throw new MembershipPasswordException("The password-answer supplied is invalid.");
        }
        #endregion
    }
}
