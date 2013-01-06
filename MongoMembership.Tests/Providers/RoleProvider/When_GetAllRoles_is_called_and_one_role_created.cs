using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_GetAllRoles_is_called_and_one_role_created : ProvidersStubs
    {
        Establish conext = () =>
        {
            roleName = "AdmiN";
            provider = CreateRoleProvider();
            provider.CreateRole(roleName);
        };

        Because of = () =>
            result = provider.GetAllRoles();

        It should_return_same_role = () =>
            result.ShouldContainOnly(new[] { roleName });

        private static MongoRoleProvider provider;
        private static string roleName;
        private static string[] result;
    }
}