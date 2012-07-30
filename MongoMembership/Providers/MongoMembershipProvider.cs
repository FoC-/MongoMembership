using System;
using System.Collections.Specialized;
using System.Configuration;
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

        public override bool EnablePasswordReset
        {
            get { return this.enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return this.enablePasswordRetrieval; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return this.maxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return this.minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return this.minRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return this.passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return this.passwordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return this.passwordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return this.requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return this.requiresUniqueEmail; }
        }
        #endregion

        #region Public Methods
        public override void Initialize(string name, NameValueCollection config)
        {
            this.ApplicationName = Util.GetValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

            this.enablePasswordReset = Util.GetValue(config["enablePasswordReset"], true);
            this.enablePasswordRetrieval = Util.GetValue(config["enablePasswordRetrieval"], false);
            this.maxInvalidPasswordAttempts = Util.GetValue(config["maxInvalidPasswordAttempts"], 5);
            this.minRequiredNonAlphanumericCharacters = Util.GetValue(config["minRequiredNonAlphanumericCharacters"], 1);
            this.minRequiredPasswordLength = Util.GetValue(config["minRequiredPasswordLength"], 7);
            this.passwordAttemptWindow = Util.GetValue(config["passwordAttemptWindow"], 10);
            this.passwordFormat = Util.GetValue(config["passwordFormat"], MembershipPasswordFormat.Hashed);
            this.passwordStrengthRegularExpression = Util.GetValue(config["passwordStrengthRegularExpression"], string.Empty);
            this.requiresQuestionAndAnswer = Util.GetValue(config["requiresQuestionAndAnswer"], false);
            this.requiresUniqueEmail = Util.GetValue(config["requiresUniqueEmail"], true);
            this.MongoConnectionString = ConnectionString(Util.GetValue(config["connectionStringKeys"], string.Empty));

            this.mongoGateway = new MongoGateway(this.MongoConnectionString);

            if (this.PasswordFormat == MembershipPasswordFormat.Hashed && this.EnablePasswordRetrieval)
            {
                throw new ProviderException("Configured settings are invalid: Hashed passwords cannot be retrieved. Either set the password format to different type, or set enablePasswordRetrieval to false.");
            }

            base.Initialize(name, config);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (username.IsNullOrWhiteSpace()) return false;

            var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);

            if (!IsPasswordCorrect(user, oldPassword))
                return false;

            var validatePasswordEventArgs = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(validatePasswordEventArgs);

            if (validatePasswordEventArgs.Cancel)
                throw new MembershipPasswordException(validatePasswordEventArgs.FailureInformation.Message);

            user.LastPasswordChangedDate = DateTime.UtcNow;
            user.Password = EncodePassword(newPassword, this.PasswordFormat, user.PasswordSalt);
            this.mongoGateway.UpdateUser(user);
            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (username.IsNullOrWhiteSpace()) return false;

            var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);

            if (!IsPasswordCorrect(user, password))
                return false;

            user.PasswordQuestion = newPasswordQuestion;
            user.PasswordAnswer = EncodePassword(newPasswordAnswer, this.PasswordFormat, user.PasswordSalt);
            this.mongoGateway.UpdateUser(user);
            return true;
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

            if (this.RequiresQuestionAndAnswer)
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

            if (this.RequiresUniqueEmail && !GetUserNameByEmail(email).IsNullOrWhiteSpace())
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
                ApplicationName = this.ApplicationName,
                CreateDate = createDate,
                EmailLowercase = email == null ? null : email.ToLowerInvariant(),
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
                Password = EncodePassword(password, this.PasswordFormat, passwordSalt),
                PasswordAnswer = EncodePassword(passwordAnswer, this.PasswordFormat, passwordSalt),
                PasswordQuestion = passwordQuestion,
                PasswordSalt = passwordSalt,
                UsernameLowercase = username == null ? null : username.ToLowerInvariant(),
                Username = username
            };

            this.mongoGateway.CreateUser(user);
            status = MembershipCreateStatus.Success;
            return GetUser(username, false);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (username.IsNullOrWhiteSpace()) return false;
            var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);

            this.mongoGateway.RemoveUser(user);
            return true;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            var users = this.mongoGateway.GetAllByEmail(this.ApplicationName, emailToMatch, pageIndex, pageSize, out totalRecords);

            foreach (var user in users)
            {
                membershipUsers.Add(ToMembershipUser(user));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            var users = this.mongoGateway.GetAllByUserName(this.ApplicationName, usernameToMatch, pageIndex, pageSize, out totalRecords);

            foreach (var user in users)
            {
                membershipUsers.Add(ToMembershipUser(user));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();
            var users = this.mongoGateway.GetAll(this.ApplicationName, pageIndex, pageSize, out totalRecords);

            foreach (var user in users)
            {
                membershipUsers.Add(ToMembershipUser(user));
            }

            return membershipUsers;
        }

        public override int GetNumberOfUsersOnline()
        {
            var timeSpan = TimeSpan.FromMinutes(Membership.UserIsOnlineTimeWindow);
            return this.mongoGateway.GetUserForPeriodOfTime(this.ApplicationName, timeSpan);
        }

        public override string GetPassword(string username, string answer)
        {
            if (!this.EnablePasswordRetrieval)
                throw new NotSupportedException("This Membership Provider has not been configured to support password retrieval.");

            var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);

            if (this.RequiresQuestionAndAnswer && !VerifyPasswordAnswer(user, answer))
                throw new MembershipPasswordException("The password-answer supplied is invalid.");

            return DecodePassword(user.Password, this.PasswordFormat);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);

            if (user == null)
                return null;

            if (userIsOnline)
            {
                user.LastActivityDate = DateTime.UtcNow;
                this.mongoGateway.UpdateUser(user);
            }

            return ToMembershipUser(user);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var user = this.mongoGateway.GetById(providerUserKey.ToString());

            if (user == null)
                return null;

            if (userIsOnline)
            {
                user.LastActivityDate = DateTime.UtcNow;
                this.mongoGateway.UpdateUser(user);
            }

            return ToMembershipUser(user);
        }

        public override string GetUserNameByEmail(string email)
        {
            var user = this.mongoGateway.GetByEmail(this.ApplicationName, email);
            return user == null ? null : user.Username;
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!this.EnablePasswordReset)
                throw new NotSupportedException("This provider is not configured to allow password resets. To enable password reset, set enablePasswordReset to \"true\" in the configuration file.");

            var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);

            if (this.RequiresQuestionAndAnswer && !VerifyPasswordAnswer(user, answer))
                throw new MembershipPasswordException("The password-answer supplied is invalid.");

            var password = Membership.GeneratePassword(this.MinRequiredPasswordLength, this.MinRequiredNonAlphanumericCharacters);
            user.LastPasswordChangedDate = DateTime.UtcNow;
            user.Password = EncodePassword(password, this.PasswordFormat, user.PasswordSalt);
            this.mongoGateway.UpdateUser(user);

            return password;
        }

        public override bool UnlockUser(string username)
        {
            if (username.IsNullOrWhiteSpace()) return false;

            var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);
            if (user == null) return false;

            user.FailedPasswordAttemptCount = 0;
            user.FailedPasswordAttemptWindowStart = new DateTime(1970, 1, 1);
            user.FailedPasswordAnswerAttemptCount = 0;
            user.FailedPasswordAnswerAttemptWindowStart = new DateTime(1970, 1, 1);
            user.IsLockedOut = false;
            user.LastLockedOutDate = new DateTime(1970, 1, 1);
            this.mongoGateway.UpdateUser(user);
            return true;
        }

        public override void UpdateUser(MembershipUser membershipUser)
        {
            var user = this.mongoGateway.GetById(membershipUser.ProviderUserKey.ToString());

            if (user == null)
                throw new ProviderException("The membershipUser was not found.");

            user.ApplicationName = this.ApplicationName;
            user.Comment = membershipUser.Comment;
            user.Email = membershipUser.Email;
            user.IsApproved = membershipUser.IsApproved;
            user.LastActivityDate = membershipUser.LastActivityDate.ToUniversalTime();
            user.LastLoginDate = membershipUser.LastLoginDate.ToUniversalTime();

            this.mongoGateway.UpdateUser(user);
        }

        public override bool ValidateUser(string username, string password)
        {
            var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);

            if (user == null || !user.IsApproved || user.IsLockedOut)
                return false;

            if (IsPasswordCorrect(user, password))
            {
                user.LastLoginDate = DateTime.UtcNow;
                this.mongoGateway.UpdateUser(user);
                return true;
            }

            user.FailedPasswordAnswerAttemptCount += 1;
            user.FailedPasswordAttemptWindowStart = DateTime.UtcNow;
            this.mongoGateway.UpdateUser(user);
            return false;
        }
        #endregion

        #region Private Methods
        private static string ConnectionString(string connectionsettingskeys)
        {
            var keys = connectionsettingskeys.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var key in keys)
            {
                var name = key.Trim(new[] { ' ' });

                if (name.IsNullOrEmpty())
                    continue;

                var connectionString = ConfigurationManager.AppSettings.Get(name);
                if (connectionString == null)
                    continue;

                return connectionString;
            }
            return "mongodb://localhost/MongoMembership";
        }

        private string DecodePassword(string password, MembershipPasswordFormat membershipPasswordFormat)
        {
            switch (membershipPasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    return password;

                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Hashed passwords cannot be decoded.");

                default:
                    var passwordBytes = Convert.FromBase64String(password);
                    var decryptedBytes = DecryptPassword(passwordBytes);
                    return decryptedBytes == null ? null : decryptedBytes.ToUnicodeString(16);
            }
        }

        private string EncodePassword(string password, MembershipPasswordFormat membershipPasswordFormat, string salt)
        {
            if (password == null)
            {
                return null;
            }

            if (membershipPasswordFormat == MembershipPasswordFormat.Clear)
            {
                return password;
            }

            var passwordBytes = password.ToByteArray();
            var saltBytes = Convert.FromBase64String(salt);
            var allBytes = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, allBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, allBytes, saltBytes.Length, passwordBytes.Length);

            if (membershipPasswordFormat == MembershipPasswordFormat.Hashed)
            {
                return allBytes.ComputeHash().ToBase64String();
            }

            return EncryptPassword(allBytes).ToBase64String();
        }

        private MembershipUser ToMembershipUser(User user)
        {
            if (user == null)
                return null;

            return new MembershipUser(Name, user.Username, user.Id, user.Email, user.PasswordQuestion, user.Comment, user.IsApproved, user.IsLockedOut, user.CreateDate, user.LastLoginDate, user.LastActivityDate, user.LastPasswordChangedDate, user.LastLockedOutDate);
        }

        private bool IsPasswordCorrect(User user, string password)
        {
            return user.Password == EncodePassword(password, this.PasswordFormat, user.PasswordSalt);
        }

        private bool VerifyPasswordAnswer(User user, string passwordAnswer)
        {
            return user.PasswordAnswer == EncodePassword(passwordAnswer, this.PasswordFormat, user.PasswordSalt);
        }
        #endregion
    }
}
