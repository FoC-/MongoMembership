using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Hosting;
using System.Web.Security;
using MongoMembership.Mongo;
using MongoMembership.Utils;

namespace MongoMembership.Providers
{
    public class MongoRoleProvider : RoleProvider
    {
        internal string MongoConnectionString { get; private set; }
        private IMongoGateway gateway;

        public override string ApplicationName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            ApplicationName = config["applicationName"].ConvertOrDefault(HostingEnvironment.ApplicationVirtualPath);

            MongoConnectionString = Util.GetConnectionStringByName(config["connectionStringKeys"].ConvertOrDefault(string.Empty));
            gateway = new MongoGateway(MongoConnectionString);

            base.Initialize(name, config);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (var roleName in roleNames.Where(roleName => !RoleExists(roleName)))
                CreateRole(roleName);

            foreach (var username in usernames)
            {
                var user = gateway.GetByUserName(ApplicationName, username);

                if (user == null)
                    throw new ProviderException($"The user '{username}' was not found.");

                var username1 = username; //Closure solving
                foreach (var roleName in roleNames.Where(roleName => !IsUserInRole(username1, roleName)))
                {
                    user.Roles.Add(roleName.ToLowerInvariant());
                    gateway.UpdateUser(user);
                }
            }
        }

        public override void CreateRole(string roleName)
        {
            if (RoleExists(roleName))
                return;

            var role = new Role
            {
                ApplicationName = ApplicationName,
                RoleName = roleName,
                RoleNameLowercased = roleName.ToLowerInvariant()
            };

            gateway.CreateRole(role);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!RoleExists(roleName))
                return false;

            var users = GetUsersInRole(roleName);

            if (throwOnPopulatedRole && users.Length > 0)
                throw new ProviderException("This role cannot be deleted because there are users present in it.");

            RemoveUsersFromRoles(users, new[] { roleName });
            gateway.RemoveRole(ApplicationName, roleName);
            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch) => RoleExists(roleName) ? gateway.GetUsersInRole(ApplicationName, roleName) : null;

        public override string[] GetAllRoles() => gateway.GetAllRoles(ApplicationName);

        public override string[] GetRolesForUser(string username) => gateway.GetRolesForUser(ApplicationName, username);

        public override string[] GetUsersInRole(string roleName) => gateway.GetUsersInRole(ApplicationName, roleName);

        public override bool IsUserInRole(string username, string roleName) => gateway.IsUserInRole(ApplicationName, username, roleName);

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (var username in usernames)
            {
                foreach (var roleName in roleNames)
                {
                    if (!IsUserInRole(username, roleName)) continue;

                    var user = gateway.GetByUserName(ApplicationName, username);
                    user.Roles.Remove(roleName.ToLowerInvariant());
                    gateway.UpdateUser(user);
                }
            }
        }

        public override bool RoleExists(string roleName) => gateway.IsRoleExists(ApplicationName, roleName);
    }
}
