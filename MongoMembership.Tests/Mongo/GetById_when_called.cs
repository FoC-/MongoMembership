using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Mongo;

namespace MongoMembership.Tests.Mongo
{
    [Subject(typeof(MongoGateway))]
    internal class GetById_when_called : StubsBase
    {
        Establish context = () =>
        {
            mongo = CreateMongoGateway();
            userCreated = CreateUser("App_Test");
            mongo.CreateUser(userCreated);
        };

        Because of = () =>
        {
            userReturned = mongo.GetById(userCreated.Id);
        };

        It should_return_user_with_same_id_as_created = () =>
            userCreated.Id.Should().BeEquivalentTo(userReturned.Id);

        private static User userReturned;
        private static User userCreated;
        private static IMongoGateway mongo;
    }
}