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
            user_created.ApplicationName.ShouldEqual(user.ApplicationName);

        It should_return_user_with_same_Id = () =>
            user_created.Id.ShouldEqual(user.Id);

        It should_return_user_with_same_Username = () =>
            user_created.Username.ShouldEqual(user.Username);

        It should_return_user_with_same_Email = () =>
            user_created.Email.ShouldEqual(user.Email);

        It should_return_user_with_same_Comment = () =>
            user_created.Comment.ShouldEqual(user.Comment);

        It should_return_user_with_same_Password = () =>
            user_created.Password.ShouldEqual(user.Password);

        It should_return_user_with_same_PasswordSalt = () =>
            user_created.PasswordSalt.ShouldEqual(user.PasswordSalt);

        It should_return_user_with_same_PasswordQuestion = () =>
            user_created.PasswordQuestion.ShouldEqual(user.PasswordQuestion);

        It should_return_user_with_same_PasswordAnswer = () =>
            user_created.PasswordAnswer.ShouldEqual(user.PasswordAnswer);

        It should_return_user_with_same_IsApproved = () =>
            user_created.IsApproved.ShouldEqual(user.IsApproved);

        It should_return_user_with_same_IsDeleted = () =>
            user_created.IsDeleted.ShouldEqual(user.IsDeleted);

        It should_return_user_with_same_IsLockedOut = () =>
            user_created.IsLockedOut.ShouldEqual(user.IsLockedOut);

        It should_return_user_with_same_IsAnonymous = () =>
            user_created.IsAnonymous.ShouldEqual(user.IsAnonymous);

        It should_return_user_with_same_LastActivityDate = () =>
            user_created.LastActivityDate.ShouldEqual(user.LastActivityDate);

        It should_return_user_with_same_LastLoginDate = () =>
            user_created.LastLoginDate.ShouldEqual(user.LastLoginDate);

        It should_return_user_with_same_LastPasswordChangedDate = () =>
            user_created.LastPasswordChangedDate.ShouldEqual(user.LastPasswordChangedDate);

        It should_return_user_with_same_CreateDate = () =>
            user_created.CreateDate.ShouldEqual(user.CreateDate);

        It should_return_user_with_same_LastLockedOutDate = () =>
            user_created.LastLockedOutDate.ShouldEqual(user.LastLockedOutDate);

        It should_return_user_with_same_LastUpdatedDate = () =>
            user_created.LastUpdatedDate.ShouldEqual(user.LastUpdatedDate);

        It should_return_user_with_same_FailedPasswordAttemptCount = () =>
            user_created.FailedPasswordAttemptCount.ShouldEqual(user.FailedPasswordAttemptCount);

        It should_return_user_with_same_FailedPasswordAttemptWindowStart = () =>
            user_created.FailedPasswordAttemptWindowStart.ShouldEqual(user.FailedPasswordAttemptWindowStart);

        It should_return_user_with_same_FailedPasswordAnswerAttemptCount = () =>
            user_created.FailedPasswordAnswerAttemptCount.ShouldEqual(user.FailedPasswordAnswerAttemptCount);

        It should_return_user_with_same_FailedPasswordAnswerAttemptWindowStart = () =>
            user_created.FailedPasswordAnswerAttemptWindowStart.ShouldEqual(user.FailedPasswordAnswerAttemptWindowStart);

        It should_return_user_with_same_Roles = () =>
            user_created.Roles.ShouldContainOnly(user.Roles);

        It should_return_user_with_same_Values = () =>
            user_created.Values.ShouldContainOnly(user.Values);

        private static IMongoGateway mongo;
        private static string applicationName;
        private static int totalRecords;
        private static User user_created;
        private static User user;
    }
}
