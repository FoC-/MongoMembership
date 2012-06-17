using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Hosting;
using System.Web.Security;
using MongoAccounting.Mongo;
using MongoAccounting.Utils;

namespace MongoAccounting.Providers
{
    public class MongoRoleProvider : RoleProvider
    {
        public override string ApplicationName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            ApplicationName = Util.GetValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);
            base.Initialize(name, config);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (var roleName in roleNames.Where(roleName => !RoleExists(roleName)))
                CreateRole(roleName);

            foreach (var username in usernames)
            {
                var user = MongoGateway.GetByUserName(ApplicationName, username);

                if (user == null)
                    throw new ProviderException("The user '{0}' was not found.".F(username));

                var username1 = username; //Closure solving
                foreach (var roleName in roleNames.Where(roleName => !IsUserInRole(username1, roleName)))
                {
                    user.Roles.Add(roleName);
                    MongoGateway.UpdateUser(user);
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
                RoleName = roleName
            };

            MongoGateway.CreateRole(role);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!RoleExists(roleName))
                return false;

            var users = GetUsersInRole(roleName);

            if (throwOnPopulatedRole && users.Length > 0)
                throw new ProviderException("This role cannot be deleted because there are users present in it.");

            RemoveUsersFromRoles(users, new[] { roleName });
            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (!RoleExists(roleName))
                return null;

            return MongoGateway.GetUsersInRole(ApplicationName, roleName);
        }

        public override string[] GetAllRoles()
        {
            return MongoGateway.GetAllRoles(ApplicationName);
        }

        public override string[] GetRolesForUser(string username)
        {
            return MongoGateway.GetRolesForUser(ApplicationName, username);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return MongoGateway.GetUsersInRole(ApplicationName, roleName);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return MongoGateway.IsUserInRole(ApplicationName, username, roleName);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (var username in usernames)
            {
                foreach (var roleName in roleNames)
                {
                    if (!IsUserInRole(username, roleName)) continue;

                    var user = MongoGateway.GetByUserName(ApplicationName, username);
                    user.Roles.Remove(roleName);
                    MongoGateway.UpdateUser(user);
                }
            }
        }

        public override bool RoleExists(string roleName)
        {
            return MongoGateway.IsRoleExists(ApplicationName, roleName);
        }
    }
}
