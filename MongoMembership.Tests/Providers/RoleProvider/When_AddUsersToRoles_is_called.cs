using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_AddUsersToRoles_is_called : ProvidersStubs
    {
        Establish conext = () =>
        {
            admin = "AdmiN";
            guest = "GueSt";
            banned = "bannned";
            username1 = "username1";
            username2 = "username2";
            username3 = "username3";

            MongoMembershipProvider membershipProvider = CreateMembershipProvider();

            provider = CreateRoleProvider();
            provider.CreateRole(admin);
            provider.CreateRole(guest);
            provider.CreateRole(banned);
            AddUser(membershipProvider, username1);
            AddUser(membershipProvider, username2);
            AddUser(membershipProvider, username3);
        };

        Because of = () =>
        {
            provider.AddUsersToRoles(new[] { username1, username2 }, new[] { admin });
            provider.AddUsersToRoles(new[] { username1, username2, username3 }, new[] { guest });
            provider.AddUsersToRoles(new[] { username3 }, new[] { banned });
        };

        It should_return_true_for__username1__and__admin__role = () =>
            provider.IsUserInRole(username1, admin).ShouldBeTrue();

        It should_return_true_for__username1__and__guest__role = () =>
            provider.IsUserInRole(username1, guest).ShouldBeTrue();

        It should_return_false_for__username1__and__banned__role = () =>
            provider.IsUserInRole(username1, banned).ShouldBeFalse();

        It should_return_false_for__username3__and__admin__role = () =>
            provider.IsUserInRole(username3, admin).ShouldBeFalse();

        It should_return_true_for__username3__and__guest__role = () =>
            provider.IsUserInRole(username3, guest).ShouldBeTrue();

        It should_return_true_for__username3__and__banned__role = () =>
            provider.IsUserInRole(username3, banned).ShouldBeTrue();

        private static MongoRoleProvider provider;
        private static string admin;
        private static string guest;
        private static string banned;
        private static string username1;
        private static string username2;
        private static string username3;
    }
}