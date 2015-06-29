using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_DeleteRole_is_called_and_role_exist : ProvidersStubs
    {
        Establish conext = () =>
        {
            roleName = "AdmiN";
            provider = CreateRoleProvider();
            provider.CreateRole(roleName);
        };

        Because of = () =>
            provider.DeleteRole(roleName, false);

        It should_delete_role = () =>
            provider.GetAllRoles().Should().BeEmpty();

        private static MongoRoleProvider provider;
        private static string roleName;
    }
}