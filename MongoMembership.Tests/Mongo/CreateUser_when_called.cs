using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Mongo;

namespace MongoMembership.Tests.Mongo
{
    [Subject(typeof(MongoGateway))]
    internal class CreateUser_when_called : StubsBase
    {
        Establish context = () =>
        {
            applicationName = "testApp";
            mongo = CreateMongoGateway();
            user = CreateUser(applicationName);
        };

        Because of = () =>
        {
            mongo.CreateUser(user);
            user_created = mongo.GetById(user.Id);
        };

        It should_return_user_with_same_ApplicationName = () =>
            user_created.ApplicationName.Should().BeEquivalentTo(user.ApplicationName);

        It should_return_user_with_same_Id = () =>
            user_created.Id.Should().BeEquivalentTo(user.Id);

        It should_return_user_with_same_Username = () =>
            user_created.Username.Should().BeEquivalentTo(user.Username);

        It should_return_user_with_same_Email = () =>
            user_created.Email.Should().BeEquivalentTo(user.Email);

        It should_return_user_with_same_Comment = () =>
            user_created.Comment.Should().BeEquivalentTo(user.Comment);

        It should_return_user_with_same_Password = () =>
            user_created.Password.Should().BeEquivalentTo(user.Password);

        It should_return_user_with_same_PasswordSalt = () =>
            user_created.PasswordSalt.Should().BeEquivalentTo(user.PasswordSalt);

        It should_return_user_with_same_PasswordQuestion = () =>
            user_created.PasswordQuestion.Should().BeEquivalentTo(user.PasswordQuestion);

        It should_return_user_with_same_PasswordAnswer = () =>
            user_created.PasswordAnswer.Should().BeEquivalentTo(user.PasswordAnswer);

        It should_return_user_with_same_IsApproved = () =>
            user_created.IsApproved.Should().Be(user.IsApproved);

        It should_return_user_with_same_IsDeleted = () =>
            user_created.IsDeleted.Should().Be(user.IsDeleted);

        It should_return_user_with_same_IsLockedOut = () =>
            user_created.IsLockedOut.Should().Be(user.IsLockedOut);

        It should_return_user_with_same_IsAnonymous = () =>
            user_created.IsAnonymous.Should().Be(user.IsAnonymous);

        It should_return_user_with_same_LastActivityDate = () =>
            user_created.LastActivityDate.Should().Be(user.LastActivityDate);

        It should_return_user_with_same_LastLoginDate = () =>
            user_created.LastLoginDate.Should().Be(user.LastLoginDate);

        It should_return_user_with_same_LastPasswordChangedDate = () =>
            user_created.LastPasswordChangedDate.Should().Be(user.LastPasswordChangedDate);

        It should_return_user_with_same_CreateDate = () =>
            user_created.CreateDate.Should().Be(user.CreateDate);

        It should_return_user_with_same_LastLockedOutDate = () =>
            user_created.LastLockedOutDate.Should().Be(user.LastLockedOutDate);

        It should_return_user_with_same_LastUpdatedDate = () =>
            user_created.LastUpdatedDate.Should().Be(user.LastUpdatedDate);

        It should_return_user_with_same_FailedPasswordAttemptCount = () =>
            user_created.FailedPasswordAttemptCount.Should().Be(user.FailedPasswordAttemptCount);

        It should_return_user_with_same_FailedPasswordAttemptWindowStart = () =>
            user_created.FailedPasswordAttemptWindowStart.Should().Be(user.FailedPasswordAttemptWindowStart);

        It should_return_user_with_same_FailedPasswordAnswerAttemptCount = () =>
            user_created.FailedPasswordAnswerAttemptCount.Should().Be(user.FailedPasswordAnswerAttemptCount);

        It should_return_user_with_same_FailedPasswordAnswerAttemptWindowStart = () =>
            user_created.FailedPasswordAnswerAttemptWindowStart.Should().Be(user.FailedPasswordAnswerAttemptWindowStart);

        It should_return_user_with_same_Roles = () =>
            user_created.Roles.Should().Contain(user.Roles);

        It should_return_user_with_same_Values = () =>
            user_created.Values.ShouldAllBeEquivalentTo(user.Values);

        private static IMongoGateway mongo;
        private static string applicationName;
        private static User user_created;
        private static User user;
    }
}
