using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_GetAllRoles_is_called_and_roles__Admin__and__AdmiN__created : ProvidersStubs
    {
        Establish conext = () =>
        {
            provider = CreateRoleProvider();
            AdmiN = "AdmiN";
            provider.CreateRole(AdmiN);
            provider.CreateRole("Admin");
        };

        Because of = () =>
            result = provider.GetAllRoles();

        It should_only__AdmiN__ = () =>
            result.ShouldContainOnly(new[] { AdmiN });

        private static MongoRoleProvider provider;
        private static string AdmiN;
        private static string[] result;
    }
}