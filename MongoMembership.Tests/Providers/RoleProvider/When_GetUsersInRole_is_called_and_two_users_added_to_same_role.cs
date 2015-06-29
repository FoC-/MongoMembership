using System.Web.Security;
using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_GetUsersInRole_is_called_and_two_users_added_to_same_role : ProvidersStubs
    {
        Establish conext = () =>
        {
            username1 = "user1";
            username2 = "user2";
            roleName = "admin";

            MongoMembershipProvider membershipProvider = CreateMembershipProvider();
            MembershipCreateStatus status;
            membershipProvider.CreateUser(username1, "password1", "ema11@il.com", null, null, true, "id-1", out status);
            membershipProvider.CreateUser(username2, "password2", "ema12@il.com", null, null, true, "id-2", out status);

            roleProvider = CreateRoleProvider();
            roleProvider.AddUsersToRoles(new[] { username1 }, new[] { roleName });
            roleProvider.AddUsersToRoles(new[] { username2 }, new[] { roleName });
        };

        Because of = () =>
        {
            result = roleProvider.GetUsersInRole(roleName);
        };

        It should_return_both_users = () =>
            result.Should().Contain(username1, username2);

        private static MongoRoleProvider roleProvider;
        private static string roleName;
        private static string username1;
        private static string username2;
        private static string[] result;
    }
}