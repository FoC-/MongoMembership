using Machine.Specifications;
using MongoMembership.Mongo;

namespace MongoMembership.Tests.Mongo
{
    [Subject(typeof(MongoGateway))]
    internal class GetById_when_called : StubsBase
    {
        private Establish context = () =>
        {
            mongo = CreateMongoGateway();
            userCreated = CreateUser("App_Test");
        };

        private Because of = () =>
        {
            mongo.CreateUser(userCreated);
            userReturned = mongo.GetById(userCreated.Id);
        };

        private It should_return_user_with_same_id_as_created = () =>
        {
            userCreated.Id.ShouldEqual(userReturned.Id);
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