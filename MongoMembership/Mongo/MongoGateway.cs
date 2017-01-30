using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoMembership.Utils;

namespace MongoMembership.Mongo
{
    internal class MongoGateway : IMongoGateway
    {
        private readonly IMongoCollection<User> usersCollection;
        private readonly IMongoCollection<Role> rolesCollection;

        public MongoGateway(string mongoConnectionString)
        {
            var mongoUrl = new MongoUrl(mongoConnectionString);
            var dataBase = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
            usersCollection = dataBase.GetCollection<User>(nameof(User));
            rolesCollection = dataBase.GetCollection<Role>(nameof(Role));

            CreateIndex();
        }

        public void DropUsers()
        {
            usersCollection.DeleteMany(u => true);
        }

        public void DropRoles()
        {
            rolesCollection.DeleteMany(r => true);
        }

        #region User

        public void CreateUser(User user)
        {
            user.UsernameLowercase = user.Username?.ToLowerInvariant();
            user.EmailLowercase = user.Email?.ToLowerInvariant();

            usersCollection.InsertOne(user);
        }

        public void UpdateUser(User user)
        {
            usersCollection.FindOneAndReplace(f => f.Id == user.Id, user);
        }

        public void RemoveUser(User user)
        {
            user.IsDeleted = true;
            UpdateUser(user);
        }

        public User GetById(string id)
        {
            return usersCollection.FindSync(f => f.Id == id).SingleOrDefault();
        }

        public User GetByUserName(string applicationName, string username)
        {
            username = username?.ToLowerInvariant();

            return usersCollection
                .FindSync(UserFilter(applicationName, u => u.UsernameLowercase == username))
                .SingleOrDefault();
        }

        public User GetByEmail(string applicationName, string email)
        {
            email = email?.ToLowerInvariant();

            return usersCollection
                .FindSync(UserFilter(applicationName, u => u.EmailLowercase == email))
                .SingleOrDefault();
        }

        public IEnumerable<User> GetAllByEmail(string applicationName, string email, int pageIndex, int pageSize,
            out int totalRecords)
        {
            email = email?.ToLowerInvariant();

            var filter = UserFilter(applicationName, u => u.UsernameLowercase.Contains(email));
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public IEnumerable<User> GetAllByUserName(string applicationName, string username, int pageIndex, int pageSize,
            out int totalRecords)
        {
            username = username?.ToLowerInvariant();

            var filter = UserFilter(applicationName, u => u.UsernameLowercase.Contains(username));
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public IEnumerable<User> GetAllAnonymByUserName(string applicationName, string username, int pageIndex,
            int pageSize, out int totalRecords)
        {
            username = username?.ToLowerInvariant();

            var filter = UserFilter(applicationName, u => u.IsAnonymous && u.UsernameLowercase.Contains(username));
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public IEnumerable<User> GetAll(string applicationName, int pageIndex, int pageSize, out int totalRecords)
        {
            var filter = UserFilter(applicationName, _ => true);
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public IEnumerable<User> GetAllAnonym(string applicationName, int pageIndex, int pageSize, out int totalRecords)
        {
            var filter = UserFilter(applicationName, u => u.IsAnonymous);
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public IEnumerable<User> GetAllInactiveSince(string applicationName, DateTime inactiveDate, int pageIndex,
            int pageSize, out int totalRecords)
        {
            var filter = UserFilter(applicationName, u => u.LastActivityDate <= inactiveDate);
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public IEnumerable<User> GetAllInactiveAnonymSince(string applicationName, DateTime inactiveDate, int pageIndex,
            int pageSize, out int totalRecords)
        {
            var filter = UserFilter(applicationName, u => u.IsAnonymous && u.LastActivityDate <= inactiveDate);
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public IEnumerable<User> GetInactiveSinceByUserName(string applicationName, string username,
            DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            username = username?.ToLowerInvariant();

            var filter = UserFilter(applicationName, u
                => u.UsernameLowercase.Contains(username)
                   && u.LastActivityDate <= userInactiveSinceDate);
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public IEnumerable<User> GetInactiveAnonymSinceByUserName(string applicationName, string username,
            DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            username = username?.ToLowerInvariant();

            var filter = UserFilter(applicationName, u
                => u.IsAnonymous
                   && u.UsernameLowercase.Contains(username)
                   && u.LastActivityDate <= userInactiveSinceDate);
            var result = GetAll(filter, pageIndex, pageSize);

            totalRecords = result.Item2;
            return result.Item1;
        }

        public int GetUserForPeriodOfTime(string applicationName, TimeSpan timeSpan)
        {
            var timeInPast = DateTime.UtcNow.Subtract(timeSpan);
            return (int)usersCollection.Count(u
               => u.ApplicationName == applicationName
                  && u.LastActivityDate > timeInPast);
        }

        #endregion

        #region Role

        public void CreateRole(Role role)
        {
            role.RoleNameLowercased = role.RoleName?.ToLowerInvariant();

            rolesCollection.InsertOne(role);
        }

        public void RemoveRole(string applicationName, string roleName)
        {
            roleName = roleName?.ToLowerInvariant();

            rolesCollection.DeleteMany(r
                => r.ApplicationName == applicationName
                   && r.RoleNameLowercased == roleName);
        }

        public string[] GetAllRoles(string applicationName)
        {
            return rolesCollection
                .FindSync(role => role.ApplicationName == applicationName)
                .ToList()
                .Select(role => role.RoleName)
                .ToArray();
        }

        public string[] GetRolesForUser(string applicationName, string username)
        {
            if (username.IsNullOrWhiteSpace())
                return null;

            var user = GetByUserName(applicationName, username);
            return user?.Roles?.ToArray();
        }

        public string[] GetUsersInRole(string applicationName, string roleName)
        {
            if (roleName.IsNullOrWhiteSpace())
                return null;

            return usersCollection
                .FindSync(user
                    => user.ApplicationName == applicationName
                       && (user.Roles.Contains(roleName.ToLowerInvariant()) || user.Roles.Contains(roleName)))
                .ToList()
                .Select(user => user.Username)
                .ToArray();
        }

        public bool IsUserInRole(string applicationName, string username, string roleName)
        {
            var user = GetByUserName(applicationName, username);
            if (user == null) return false;

            return user.Roles.Any(r => string.Equals(r, roleName, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool IsRoleExists(string applicationName, string roleName)
        {
            roleName = roleName?.ToLowerInvariant();

            return rolesCollection.FindSync(RoleFilter(applicationName, r => r.RoleNameLowercased == roleName)).Any();
        }

        #endregion

        private static FilterDefinition<User> UserFilter(string applicationName, Expression<Func<User, bool>> extraFilter)
        {
            return Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.IsDeleted, false),
                Builders<User>.Filter.Eq(u => u.ApplicationName, applicationName),
                Builders<User>.Filter.Where(extraFilter)
            );
        }

        private static FilterDefinition<Role> RoleFilter(string applicationName, Expression<Func<Role, bool>> extraFilter)
        {
            return Builders<Role>.Filter.And(
                Builders<Role>.Filter.Eq(u => u.ApplicationName, applicationName),
                Builders<Role>.Filter.Where(extraFilter)
            );
        }

        private Tuple<ICollection<User>, int> GetAll(FilterDefinition<User> filter, int pageIndex, int pageSize)
        {
            var options = new FindOptions<User>
            {
                Skip = pageIndex * pageSize,
                Limit = pageSize,
            };
            var users = usersCollection.FindSync(filter, options).ToList();
            var total = (int)usersCollection.Count(filter);
            return new Tuple<ICollection<User>, int>(users, total);
        }

        private void CreateIndex()
        {
            var userApplicationNameKey = Builders<User>.IndexKeys.Text(_ => _.ApplicationName);
            var emailLowercaseKey = Builders<User>.IndexKeys.Text(_ => _.EmailLowercase);
            var usernameLowercaseKey = Builders<User>.IndexKeys.Text(_ => _.UsernameLowercase);
            var rolesKey = Builders<User>.IndexKeys.Text(_ => _.Roles);
            var isAnonymousKey = Builders<User>.IndexKeys.Ascending(_ => _.Roles);
            var lastActivityDateKey = Builders<User>.IndexKeys.Ascending(_ => _.LastActivityDate);

            usersCollection.Indexes.CreateMany(new[]
            {
                new CreateIndexModel<User>(userApplicationNameKey),
                ModelFor(userApplicationNameKey, emailLowercaseKey),
                ModelFor(userApplicationNameKey, usernameLowercaseKey),
                ModelFor(userApplicationNameKey, rolesKey),
                ModelFor(userApplicationNameKey, rolesKey, usernameLowercaseKey),
                ModelFor(userApplicationNameKey, isAnonymousKey),
                ModelFor(userApplicationNameKey, isAnonymousKey, lastActivityDateKey),
                ModelFor(userApplicationNameKey, isAnonymousKey, lastActivityDateKey, usernameLowercaseKey),
                ModelFor(userApplicationNameKey, isAnonymousKey, usernameLowercaseKey),
                ModelFor(userApplicationNameKey, lastActivityDateKey)
            });

            var roleApplicationNameKey = Builders<Role>.IndexKeys.Text(_ => _.ApplicationName);
            var roleNameLowercasedKey = Builders<Role>.IndexKeys.Text(_ => _.RoleNameLowercased);
            rolesCollection.Indexes.CreateMany(new[]
            {
                new CreateIndexModel<Role>(roleApplicationNameKey),
                ModelFor(roleApplicationNameKey, roleNameLowercasedKey)
            });
        }

        private static CreateIndexModel<T> ModelFor<T>(params IndexKeysDefinition<T>[] keys)
        {
            return new CreateIndexModel<T>(Builders<T>.IndexKeys.Combine(keys));
        }
    }
}