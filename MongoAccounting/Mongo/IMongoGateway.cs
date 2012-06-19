using System;
using System.Collections.Generic;

namespace MongoAccounting.Mongo
{
    internal interface IMongoGateway
    {
        string MongoConnectionString { get; }
        void DropUsers();
        void DropRoles();
        void CreateUser(User user);
        void UpdateUser(User user);
        void RemoveUser(User user);
        User GetById(string id);
        User GetByUserName(string applicationName, string username);
        User GetByEmail(string applicationName, string email);
        IEnumerable<User> GetAllByEmail(string applicationName, string email, int pageIndex, int pageSize, out int totalRecords);
        IEnumerable<User> GetAllByUserName(string applicationName, string username, int pageIndex, int pageSize, out int totalRecords);
        IEnumerable<User> GetAllAnonymByUserName(string applicationName, string username, int pageIndex, int pageSize, out int totalRecords);
        IEnumerable<User> GetAll(string applicationName, int pageIndex, int pageSize, out int totalRecords);
        IEnumerable<User> GetAllAnonym(string applicationName, int pageIndex, int pageSize, out int totalRecords);
        IEnumerable<User> GetAllInactiveSince(string applicationName, DateTime inactiveDate, int pageIndex, int pageSize, out int totalRecords);
        IEnumerable<User> GetAllInactiveAnonymSince(string applicationName, DateTime inactiveDate, int pageIndex, int pageSize, out int totalRecords);
        IEnumerable<User> GetInactiveSinceByUserName(string applicationName, string username, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords);
        IEnumerable<User> GetInactiveAnonymSinceByUserName(string applicationName, string username, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords);
        int GetUserForPeriodOfTime(string applicationName, TimeSpan timeSpan);
        void CreateRole(Role role);
        void RemoveRole(string applicationName, string roleName);
        string[] GetAllRoles(string applicationName);
        string[] GetRolesForUser(string applicationName, string username);
        string[] GetUsersInRole(string applicationName, string roleName);
        bool IsUserInRole(string applicationName, string username, string roleName);
        bool IsRoleExists(string applicationName, string roleName);
    }
}