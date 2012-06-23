using System;
using System.Collections.Specialized;
using System.Configuration;
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
        private IMongoGateway mongoGateway;

        public override string ApplicationName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            this.ApplicationName = Util.GetValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

            this.MongoConnectionString = ConnectionString(Util.GetValue(config["connectionStringKeys"], string.Empty));
            this.mongoGateway = new MongoGateway(MongoConnectionString);

            base.Initialize(name, config);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (var roleName in roleNames.Where(roleName => !RoleExists(roleName)))
                CreateRole(roleName);

            foreach (var username in usernames)
            {
                var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);

                if (user == null)
                    throw new ProviderException("The user '{0}' was not found.".F(username));

                var username1 = username; //Closure solving
                foreach (var roleName in roleNames.Where(roleName => !IsUserInRole(username1, roleName)))
                {
                    user.Roles.Add(roleName);
                    this.mongoGateway.UpdateUser(user);
                }
            }
        }

        public override void CreateRole(string roleName)
        {
            if (RoleExists(roleName))
                return;

            var role = new Role
            {
                ApplicationName = this.ApplicationName,
                RoleName = roleName
            };

            this.mongoGateway.CreateRole(role);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!RoleExists(roleName))
                return false;

            var users = GetUsersInRole(roleName);

            if (throwOnPopulatedRole && users.Length > 0)
                throw new ProviderException("This role cannot be deleted because there are users present in it.");

            RemoveUsersFromRoles(users, new[] { roleName });
            this.mongoGateway.RemoveRole(this.ApplicationName, roleName);
            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (!RoleExists(roleName))
                return null;

            return this.mongoGateway.GetUsersInRole(this.ApplicationName, roleName);
        }

        public override string[] GetAllRoles()
        {
            return this.mongoGateway.GetAllRoles(this.ApplicationName);
        }

        public override string[] GetRolesForUser(string username)
        {
            return this.mongoGateway.GetRolesForUser(this.ApplicationName, username);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return this.mongoGateway.GetUsersInRole(this.ApplicationName, roleName);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return this.mongoGateway.IsUserInRole(this.ApplicationName, username, roleName);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (var username in usernames)
            {
                foreach (var roleName in roleNames)
                {
                    if (!IsUserInRole(username, roleName)) continue;

                    var user = this.mongoGateway.GetByUserName(this.ApplicationName, username);
                    user.Roles.Remove(roleName);
                    this.mongoGateway.UpdateUser(user);
                }
            }
        }

        public override bool RoleExists(string roleName)
        {
            return this.mongoGateway.IsRoleExists(this.ApplicationName, roleName);
        }

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
        #endregion
    }
}
