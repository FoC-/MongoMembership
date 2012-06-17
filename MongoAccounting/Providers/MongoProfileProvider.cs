using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.Hosting;
using System.Web.Profile;
using MongoAccounting.Mongo;
using MongoAccounting.Utils;

namespace MongoAccounting.Providers
{
    public class MongoProfileProvider : ProfileProvider
    {
        public override string ApplicationName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            ApplicationName = Util.GetValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

            base.Initialize(name, config);
        }

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int totalRecords;
            ProfileInfoCollection profileInfoCollection = GetAllInactiveProfiles(authenticationOption,
                                                                                 userInactiveSinceDate,
                                                                                 0,
                                                                                 int.MaxValue,
                                                                                 out totalRecords);

            return DeleteProfiles(profileInfoCollection);
        }

        public override int DeleteProfiles(string[] usernames)
        {
            int countAffected = 0;

            foreach (User user in usernames.Select(username => MongoGateway.GetByUserName(ApplicationName, username)))
            {
                MongoGateway.RemoveUser(user);
                countAffected++;
            }

            return countAffected;
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            return DeleteProfiles(profiles
                                    .Cast<ProfileInfo>()
                                    .Select(profile => profile.UserName)
                                    .ToArray());
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;

            var users = Enumerable.Empty<User>();

            switch (authenticationOption)
            {
                case ProfileAuthenticationOption.Anonymous:
                    users = MongoGateway.GetInactiveAnonymSinceByUserName(ApplicationName, usernameToMatch, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
                    break;
                case ProfileAuthenticationOption.Authenticated:
                case ProfileAuthenticationOption.All:
                    users = MongoGateway.GetInactiveSinceByUserName(ApplicationName, usernameToMatch, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
                    break;
            }

            return ToProfileInfoCollection(users);
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;

            var users = Enumerable.Empty<User>();

            switch (authenticationOption)
            {
                case ProfileAuthenticationOption.Anonymous:
                    users = MongoGateway.GetAllAnonymByUserName(ApplicationName, usernameToMatch, pageIndex, pageSize, out totalRecords);
                    break;
                case ProfileAuthenticationOption.Authenticated:
                case ProfileAuthenticationOption.All:
                    users = MongoGateway.GetAllByUserName(ApplicationName, usernameToMatch, pageIndex, pageSize, out totalRecords);
                    break;
            }

            return ToProfileInfoCollection(users);
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            var users = Enumerable.Empty<User>();

            switch (authenticationOption)
            {
                case ProfileAuthenticationOption.Anonymous:
                    users = MongoGateway.GetAllInactiveAnonymSince(ApplicationName, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
                    break;
                case ProfileAuthenticationOption.Authenticated:
                case ProfileAuthenticationOption.All:
                    users = MongoGateway.GetAllInactiveSince(ApplicationName, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
                    break;
            }

            return ToProfileInfoCollection(users);
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;

            var users = Enumerable.Empty<User>();

            switch (authenticationOption)
            {
                case ProfileAuthenticationOption.Anonymous:
                    users = MongoGateway.GetAllAnonym(ApplicationName, pageIndex, pageSize, out totalRecords);
                    break;
                case ProfileAuthenticationOption.Authenticated:
                case ProfileAuthenticationOption.All:
                    users = MongoGateway.GetAll(ApplicationName, pageIndex, pageSize, out totalRecords);
                    break;
            }

            return ToProfileInfoCollection(users);
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int numberOfInactiveProfiles;

            GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, 0, int.MaxValue, out numberOfInactiveProfiles);

            return numberOfInactiveProfiles;
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            var settingsPropertyValueCollection = new SettingsPropertyValueCollection();

            if (context == null || collection == null || collection.Count < 1)
            {
                return settingsPropertyValueCollection;
            }

            var username = (string)context["UserName"];

            if (username.IsNullOrWhiteSpace() || collection.Count < 1)
                return settingsPropertyValueCollection;

            User user = MongoGateway.GetByUserName(ApplicationName, username);

            foreach (SettingsProperty property in collection)
            {
                var propertyValue = new SettingsPropertyValue(property);

                if (user.Values.ContainsKey(propertyValue.Name))
                {
                    propertyValue.PropertyValue = user.Values[propertyValue.Name];
                    propertyValue.IsDirty = false;
                    propertyValue.Deserialized = true;
                }

                settingsPropertyValueCollection.Add(propertyValue);
            }

            user.LastActivityDate = DateTime.Now;
            MongoGateway.UpdateUser(user);

            return settingsPropertyValueCollection;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            var username = (string)context["UserName"];
            var isAuthenticated = (bool)context["IsAuthenticated"];

            if (username.IsNullOrWhiteSpace() || collection.Count < 1)
                return;

            var values = new Dictionary<string, object>();

            foreach (SettingsPropertyValue value in collection)
            {
                if (!value.IsDirty || (!isAuthenticated && !(bool)value.Property.Attributes["AllowAnonymous"]))
                    continue;

                values.Add(value.Name, value.PropertyValue);
            }

            var user = MongoGateway.GetByUserName(ApplicationName, username)
                    ?? new User
                    {
                        ApplicationName = ApplicationName,
                        Username = username
                    };
            user.LastActivityDate = DateTime.Now;
            user.LastUpdatedDate = DateTime.Now;
            user.AddValues(values);
            MongoGateway.UpdateUser(user);
        }

        private static ProfileInfo ToProfileInfo(User user)
        {
            return new ProfileInfo(user.Username, user.IsAnonymous, user.LastActivityDate, user.LastUpdatedDate, 0);
        }

        private static ProfileInfoCollection ToProfileInfoCollection(IEnumerable<User> users)
        {
            var profileCollection = new ProfileInfoCollection();

            foreach (var user in users)
            {
                profileCollection.Add(ToProfileInfo(user));
            }

            return profileCollection;
        }
    }
}