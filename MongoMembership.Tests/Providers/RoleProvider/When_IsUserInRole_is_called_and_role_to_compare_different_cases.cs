using System.Web.Security;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_IsUserInRole_is_called_and_role_to_compare_different_cases : StubsBase
    {
        private Establish conext = () =>
        {
            roleName = "AdmiN";
            username = "UserName";

            membershipProvider = CreateMembershipProvider();
            MembershipCreateStatus status;
            membershipProvider.CreateUser(username, "PASSWORD!1", "EM@ai.lll", null, null, true, "key111", out status);

            provider = CreateRoleProvider();
            provider.AddUsersToRoles(new[] { username }, new[] { roleName });
        };

        private Because of = () =>
            result = provider.IsUserInRole(username, roleName.ToUpperInvariant());

        private It should_return_true = () =>
            result.ShouldBeTrue();

        private Cleanup staff = () =>
        {
            membershipProvider.DeleteUser(username, true);
            provider.DeleteRole(roleName, false);
        };

        private static bool result;
        private static MongoRoleProvider provider;
        private static string roleName;
        private static string username;
        private static MongoMembershipProvider membershipProvider;
    }
}