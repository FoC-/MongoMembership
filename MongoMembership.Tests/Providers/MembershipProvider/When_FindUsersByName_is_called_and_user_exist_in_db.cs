using System;
using System.Web.Security;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.MembershipProvider
{
    class When_FindUsersByName_is_called_and_user_exist_in_db : StubsBase
    {
        private Establish context = () =>
        {
            provider = CreateProvider();
            userName = "UserName";

            MembershipCreateStatus status;
            provider.CreateUser(userName, "aqswdefr1", "email@email.com", null, null, true, Guid.NewGuid(), out status);
        };

        private Because of = () =>
        {
            int totalRecords;
            users = provider.FindUsersByName(userName, 0, 10, out totalRecords);
        };

        private It should_return_user = () =>
        {
            users.ShouldNotBeNull();
        };

        private Cleanup staff = () =>
        {
            int total;
            MembershipUserCollection allUsers = provider.GetAllUsers(1, 100, out total);

            foreach (MembershipUser user in allUsers)
            {
                provider.DeleteUser(user.UserName, true);
            }
        };

        private static MembershipUserCollection users;
        private static MongoMembershipProvider provider;
        private static string userName;
    }
}