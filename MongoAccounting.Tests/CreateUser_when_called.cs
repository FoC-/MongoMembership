using Machine.Specifications;
using MongoAccounting.Mongo;

namespace MongoAccounting.Tests
{
    [Subject(typeof(MongoGateway))]
    public class CreateUser_when_called : StubsBase
    {
        private Establish context = () =>
        {
            applicationName = "testApp";
            mongo = CreateMongoGateway();
        };

        private Because of = () =>
        {
            mongo.CreateUser(CreateUser(applicationName));
            mongo.GetAll(applicationName, 0, int.MaxValue, out totalRecords);
        };

        private It should_return_only_one_user = () =>
        {
            totalRecords.ShouldEqual(1);
        };

        private Cleanup all = () =>
        {
            mongo.DropUsers();
            mongo.DropRoles();
        };

        private static IMongoGateway mongo;
        private static string applicationName;
        private static int totalRecords;
    }
}
