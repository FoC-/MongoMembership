using Machine.Specifications;
using MongoAccounting.Mongo;

namespace MongoAccounting.Tests
{
    [Subject(typeof(MongoGateway))]
    internal class GetById_when_called : StubsBase
    {
        private Establish context = () =>
        {
            mongo = CreateMongoGateway();
            userCreated = CreateUser("test_app");
        };

        private Because of = () =>
        {
            mongo.CreateUser(userCreated);
            userReturned = mongo.GetById(userCreated.Id);
        };

        private It should_return_user_with_same_id_as_created = () =>
        {
            userReturned.Id.ShouldEqual(userCreated.Id);
        };

        private Cleanup all = () =>
        {
            mongo.DropRoles();
            mongo.DropUsers();
        };

        private static User userReturned;
        private static User userCreated;
        private static IMongoGateway mongo;
    }
}