using Machine.Specifications;
using MongoMembership.Mongo;

namespace MongoMembership.Tests.Mongo
{
    [Subject(typeof(MongoGateway))]
    internal class IsRoleExists_when_called_and_search_with_lovercased_role_name : StubsBase
    {
        Establish context = () =>
        {
            mongo = CreateMongoGateway();

            role = CreateRole();
            mongo.CreateRole(role);
        };

        Because of = () =>
        {
            isRoleExists = mongo.IsRoleExists(role.ApplicationName, role.RoleName.ToLowerInvariant());
        };

        It should_be_found = () =>
        {
            isRoleExists.ShouldEqual(true);
        };

        Cleanup staff = () =>
        {
            mongo.DropRoles();
            mongo.DropUsers();
        };

        private static bool isRoleExists;
        private static string roleName;
        private static IMongoGateway mongo;
        private static Role role;
    }
}