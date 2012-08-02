using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoMembership.Utils;

namespace MongoMembership.Mongo
{
    internal class MongoGateway : IMongoGateway
    {
        public string MongoConnectionString { get; private set; }

        private MongoDatabase DataBase
        {
            get { return MongoDatabase.Create(MongoConnectionString); }
        }
        private MongoCollection<User> UsersCollection
        {
            get { return DataBase.GetCollection<User>(typeof(User).Name); }
        }
        private MongoCollection<Role> RolesCollection
        {
            get { return DataBase.GetCollection<Role>(typeof(Role).Name); }
        }

        static MongoGateway()
        {
            RegisterClassMapping();
        }

        public MongoGateway(string mongoConnectionString)
        {
            MongoConnectionString = mongoConnectionString;
            CreateIndex();
        }

        public void DropUsers()
        {
            UsersCollection.Drop();
        }

        public void DropRoles()
        {
            RolesCollection.Drop();
        }

        #region User
        public void CreateUser(User user)
        {
            if (user.Username != null) user.UsernameLowercase = user.Username.ToLowerInvariant();
            if (user.Email != null) user.EmailLowercase = user.Email.ToLowerInvariant();

            UsersCollection.Insert(user);
        }

        public void UpdateUser(User user)
        {
            UsersCollection.Save(user);
        }

        public void RemoveUser(User user)
        {
            user.IsDeleted = true;
            UpdateUser(user);
        }

        public User GetById(string id)
        {
            if (id.IsNullOrWhiteSpace() || UsersCollection.Count() == 0)
                return null;

            return UsersCollection.FindOneById(id);
        }

        public User GetByUserName(string applicationName, string username)
        {
            if (username.IsNullOrWhiteSpace() || UsersCollection.Count() == 0)
                return null;

            return UsersCollection
                    .AsQueryable()
                    .SingleOrDefault(user
                        => user.ApplicationName == applicationName
                        && user.UsernameLowercase == username.ToLowerInvariant()
                        && user.IsDeleted == false);
        }

        public User GetByEmail(string applicationName, string email)
        {
            if (email.IsNullOrWhiteSpace() || UsersCollection.Count() == 0)
                return null;

            return UsersCollection
                    .AsQueryable()
                    .SingleOrDefault(user
                        => user.ApplicationName == applicationName
                        && user.EmailLowercase == email.ToLowerInvariant()
                        && user.IsDeleted == false);
        }

        public IEnumerable<User> GetAllByEmail(string applicationName, string email, int pageIndex, int pageSize, out int totalRecords)
        {
            if (email.IsNullOrWhiteSpace() || UsersCollection.Count() == 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<User>();
            }

            var users = UsersCollection
                        .AsQueryable()
                        .Where(user
                            => user.ApplicationName == applicationName
                            && user.EmailLowercase.Contains(email.ToLowerInvariant())
                            && user.IsDeleted == false);

            totalRecords = users.Count();
            return users.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<User> GetAllByUserName(string applicationName, string username, int pageIndex, int pageSize, out int totalRecords)
        {
            if (username.IsNullOrWhiteSpace() || UsersCollection.Count() == 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<User>();
            }

            var users = UsersCollection
                        .AsQueryable()
                        .Where(user
                            => user.ApplicationName == applicationName
                            && user.UsernameLowercase.Contains(username.ToLowerInvariant())
                            && user.IsDeleted == false);

            totalRecords = users.Count();
            return users.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<User> GetAllAnonymByUserName(string applicationName, string username, int pageIndex, int pageSize, out int totalRecords)
        {
            if (username.IsNullOrWhiteSpace() || UsersCollection.Count() == 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<User>();
            }

            var users = UsersCollection
                .AsQueryable()
                .Where(user
                    => user.ApplicationName == applicationName
                    && user.UsernameLowercase.Contains(username.ToLowerInvariant())
                    && user.IsAnonymous
                    && user.IsDeleted == false);

            totalRecords = users.Count();
            return users.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<User> GetAll(string applicationName, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = (int)UsersCollection.Count();

            if (totalRecords == 0)
                return Enumerable.Empty<User>();

            return UsersCollection
                    .AsQueryable()
                    .Where(user
                        => user.ApplicationName == applicationName
                        && user.IsDeleted == false)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);
        }

        public IEnumerable<User> GetAllAnonym(string applicationName, int pageIndex, int pageSize, out int totalRecords)
        {
            if (UsersCollection.Count() == 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<User>();
            }

            var users = UsersCollection
                .AsQueryable()
                .Where(user
                    => user.ApplicationName == applicationName
                    && user.IsAnonymous
                    && user.IsDeleted == false);

            totalRecords = users.Count();
            return users.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<User> GetAllInactiveSince(string applicationName, DateTime inactiveDate, int pageIndex, int pageSize, out int totalRecords)
        {
            if (UsersCollection.Count() == 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<User>();
            }

            var users = UsersCollection
                        .AsQueryable()
                        .Where(user
                            => user.ApplicationName == applicationName
                            && user.LastActivityDate <= inactiveDate
                            && user.IsDeleted == false);
            totalRecords = users.Count();
            return users
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);
        }

        public IEnumerable<User> GetAllInactiveAnonymSince(string applicationName, DateTime inactiveDate, int pageIndex, int pageSize, out int totalRecords)
        {
            if (UsersCollection.Count() == 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<User>();
            }

            var users = UsersCollection
                        .AsQueryable()
                        .Where(user
                            => user.ApplicationName == applicationName
                            && user.LastActivityDate <= inactiveDate
                            && user.IsAnonymous
                            && user.IsDeleted == false);
            totalRecords = users.Count();
            return users
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);
        }

        public IEnumerable<User> GetInactiveSinceByUserName(string applicationName, string username, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            if (UsersCollection.Count() == 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<User>();
            }

            var users = UsersCollection
                        .AsQueryable()
                        .Where(user
                            => user.ApplicationName == applicationName
                            && user.UsernameLowercase.Contains(username.ToLowerInvariant())
                            && user.LastActivityDate <= userInactiveSinceDate
                            && user.IsDeleted == false);
            totalRecords = users.Count();
            return users
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);
        }

        public IEnumerable<User> GetInactiveAnonymSinceByUserName(string applicationName, string username, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            if (UsersCollection.Count() == 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<User>();
            }

            var users = UsersCollection
                        .AsQueryable()
                        .Where(user
                            => user.ApplicationName == applicationName
                            && user.UsernameLowercase.Contains(username.ToLowerInvariant())
                            && user.LastActivityDate <= userInactiveSinceDate
                            && user.IsAnonymous
                            && user.IsDeleted == false);
            totalRecords = users.Count();
            return users
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);
        }

        public int GetUserForPeriodOfTime(string applicationName, TimeSpan timeSpan)
        {
            return UsersCollection
                    .AsQueryable()
                    .Count(user
                        => user.ApplicationName == applicationName
                        && user.LastActivityDate > DateTime.UtcNow.Subtract(timeSpan));
        }
        #endregion

        #region Role
        public void CreateRole(Role role)
        {
            if (role.RoleName != null) role.RoleNameLowercased = role.RoleName.ToLowerInvariant();

            RolesCollection.Insert(role);
        }

        public void RemoveRole(string applicationName, string roleName)
        {
            var query = new QueryDocument
            {
                new Dictionary<string, object>
                {
                    {Util.GetElementNameFor<Role>(_ => _.ApplicationName), applicationName},
                    {Util.GetElementNameFor<Role>(_ => _.RoleNameLowercased), roleName.ToLowerInvariant()}
                }
            };

            RolesCollection.Remove(query);
        }

        public string[] GetAllRoles(string applicationName)
        {
            return RolesCollection
                    .AsQueryable()
                    .Where(role => role.ApplicationName == applicationName)
                    .Select(role => role.RoleName)
                    .ToArray();
        }

        public string[] GetRolesForUser(string applicationName, string username)
        {
            if (username.IsNullOrWhiteSpace())
                return null;

            User user = GetByUserName(applicationName, username);

            if (user == null || user.Roles == null)
                return null;

            return user.Roles.ToArray();
        }

        public string[] GetUsersInRole(string applicationName, string roleName)
        {
            if (roleName.IsNullOrWhiteSpace())
                return null;

            return UsersCollection
                    .AsQueryable()
                    .Where(user
                        => user.ApplicationName == applicationName
                        && (user.Roles.Contains(roleName.ToLowerInvariant()) || user.Roles.Contains(roleName)))
                    .Select(user => user.Username)
                    .ToArray();
        }

        public bool IsUserInRole(string applicationName, string username, string roleName)
        {
            if (username.IsNullOrWhiteSpace() || roleName.IsNullOrWhiteSpace())
                return false;

            return UsersCollection
                    .AsQueryable()
                    .Any(user
                        => user.ApplicationName == applicationName
                        && user.UsernameLowercase == username.ToLowerInvariant()
                        && (user.Roles.Contains(roleName.ToLowerInvariant())) || user.Roles.Contains(roleName));
        }

        public bool IsRoleExists(string applicationName, string roleName)
        {
            if (roleName.IsNullOrWhiteSpace())
                return false;

            return RolesCollection
                    .AsQueryable()
                    .Any(role
                        => role.ApplicationName == applicationName
                        && role.RoleNameLowercased == roleName.ToLowerInvariant());
        }
        #endregion

        #region Private Methods
        private static void RegisterClassMapping()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
            {
                // Initialize Mongo Mappings
                BsonClassMap.RegisterClassMap<User>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.SetIsRootClass(true);
                    cm.MapIdField(c => c.Id);
                    cm.MapProperty(c => c.ApplicationName).SetElementName("ApplicationName");
                    cm.MapProperty(c => c.Username).SetElementName("Username");
                    cm.MapProperty(c => c.UsernameLowercase).SetElementName("UsernameLowercase");
                    cm.MapProperty(c => c.Comment).SetElementName("Comment");
                    cm.MapProperty(c => c.CreateDate).SetElementName("CreateDate");
                    cm.MapProperty(c => c.Email).SetElementName("Email");
                    cm.MapProperty(c => c.EmailLowercase).SetElementName("EmailLowercase");
                    cm.MapProperty(c => c.FailedPasswordAnswerAttemptCount).SetElementName("FailedPasswordAnswerAttemptCount");
                    cm.MapProperty(c => c.FailedPasswordAttemptCount).SetElementName("FailedPasswordAttemptCount");
                    cm.MapProperty(c => c.FailedPasswordAnswerAttemptWindowStart).SetElementName("FailedPasswordAnswerAttemptWindowStart");
                    cm.MapProperty(c => c.FailedPasswordAttemptWindowStart).SetElementName("FailedPasswordAttemptWindowStart");
                    cm.MapProperty(c => c.IsApproved).SetElementName("IsApproved");
                    cm.MapProperty(c => c.IsDeleted).SetElementName("IsDeleted");
                    cm.MapProperty(c => c.IsLockedOut).SetElementName("IsLockedOut");
                    cm.MapProperty(c => c.LastActivityDate).SetElementName("LastActivityDate");
                    cm.MapProperty(c => c.LastLockedOutDate).SetElementName("LastLockedOutDate");
                    cm.MapProperty(c => c.LastLoginDate).SetElementName("LastLoginDate");
                    cm.MapProperty(c => c.LastPasswordChangedDate).SetElementName("LastPasswordChangedDate");
                    cm.MapProperty(c => c.Password).SetElementName("Password");
                    cm.MapProperty(c => c.PasswordAnswer).SetElementName("PasswordAnswer");
                    cm.MapProperty(c => c.PasswordQuestion).SetElementName("PasswordQuestion");
                    cm.MapProperty(c => c.PasswordSalt).SetElementName("PasswordSalt");
                    cm.MapProperty(c => c.Roles).SetElementName("Roles").SetIgnoreIfNull(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Role)))
            {
                BsonClassMap.RegisterClassMap<Role>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.SetIsRootClass(true);
                    cm.MapProperty(c => c.ApplicationName).SetElementName("ApplicationName");
                    cm.MapProperty(c => c.RoleName).SetElementName("RoleName");
                    cm.MapProperty(c => c.RoleNameLowercased).SetElementName("RoleNameLowercased");
                });
            }
        }

        private void CreateIndex()
        {
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.EmailLowercase));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.UsernameLowercase));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.Roles));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.Roles), Util.GetElementNameFor<User>(_ => _.UsernameLowercase));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.IsAnonymous));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.IsAnonymous), Util.GetElementNameFor<User>(_ => _.LastActivityDate));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.IsAnonymous), Util.GetElementNameFor<User>(_ => _.LastActivityDate), Util.GetElementNameFor<User>(_ => _.UsernameLowercase));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.IsAnonymous), Util.GetElementNameFor<User>(_ => _.UsernameLowercase));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.UsernameLowercase), Util.GetElementNameFor<User>(_ => _.IsAnonymous));
            UsersCollection.EnsureIndex(Util.GetElementNameFor<User>(_ => _.ApplicationName), Util.GetElementNameFor<User>(_ => _.LastActivityDate));

            RolesCollection.EnsureIndex(Util.GetElementNameFor<Role>(_ => _.ApplicationName));
            RolesCollection.EnsureIndex(Util.GetElementNameFor<Role>(_ => _.ApplicationName), Util.GetElementNameFor<Role>(_ => _.RoleNameLowercased));
        }
        #endregion
    }
}