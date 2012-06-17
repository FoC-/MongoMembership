using System.Linq;
using System.Collections.Generic;
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
            user = CreateUser(applicationName);
        };

        private Because of = () =>
        {
            mongo.CreateUser(user);
            users = mongo.GetAll(applicationName, 0, int.MaxValue, out totalRecords);
        };

        private It should_return_only_one_user = () =>
        {
            totalRecords.ShouldEqual(1);
        };

        private It should_return_user_with_same_ApplicationName = () =>
        {
            users.Single().ApplicationName.ShouldEqual(user.ApplicationName);
        };
        private It should_return_user_with_same_Id = () =>
        {
            users.Single().Id.ShouldEqual(user.Id);
        };
        private It should_return_user_with_same_Username = () =>
        {
            users.Single().Username.ShouldEqual(user.Username);
        };
        private It should_return_user_with_same_Email = () =>
        {
            users.Single().Email.ShouldEqual(user.Email);
        };
        private It should_return_user_with_same_Comment = () =>
        {
            users.Single().Comment.ShouldEqual(user.Comment);
        };
        private It should_return_user_with_same_Password = () =>
        {
            users.Single().Password.ShouldEqual(user.Password);
        };
        private It should_return_user_with_same_PasswordSalt = () =>
        {
            users.Single().PasswordSalt.ShouldEqual(user.PasswordSalt);
        };
        private It should_return_user_with_same_PasswordQuestion = () =>
        {
            users.Single().PasswordQuestion.ShouldEqual(user.PasswordQuestion);
        };
        private It should_return_user_with_same_PasswordAnswer = () =>
        {
            users.Single().PasswordAnswer.ShouldEqual(user.PasswordAnswer);
        };
        private It should_return_user_with_same_IsApproved = () =>
        {
            users.Single().IsApproved.ShouldEqual(user.IsApproved);
        };
        private It should_return_user_with_same_IsDeleted = () =>
        {
            users.Single().IsDeleted.ShouldEqual(user.IsDeleted);
        };
        private It should_return_user_with_same_IsLockedOut = () =>
        {
            users.Single().IsLockedOut.ShouldEqual(user.IsLockedOut);
        };
        private It should_return_user_with_same_IsAnonymous = () =>
        {
            users.Single().IsAnonymous.ShouldEqual(user.IsAnonymous);
        };
        private It should_return_user_with_same_LastActivityDate = () =>
        {
            users.Single().LastActivityDate.ShouldEqual(user.LastActivityDate);
        };
        private It should_return_user_with_same_LastLoginDate = () =>
        {
            users.Single().LastLoginDate.ShouldEqual(user.LastLoginDate);
        };
        private It should_return_user_with_same_LastPasswordChangedDate = () =>
        {
            users.Single().LastPasswordChangedDate.ShouldEqual(user.LastPasswordChangedDate);
        };
        private It should_return_user_with_same_CreateDate = () =>
        {
            users.Single().CreateDate.ShouldEqual(user.CreateDate);
        };
        private It should_return_user_with_same_LastLockedOutDate = () =>
        {
            users.Single().LastLockedOutDate.ShouldEqual(user.LastLockedOutDate);
        };
        private It should_return_user_with_same_LastUpdatedDate = () =>
        {
            users.Single().LastUpdatedDate.ShouldEqual(user.LastUpdatedDate);
        };
        private It should_return_user_with_same_FailedPasswordAttemptCount = () =>
        {
            users.Single().FailedPasswordAttemptCount.ShouldEqual(user.FailedPasswordAttemptCount);
        };
        private It should_return_user_with_same_FailedPasswordAttemptWindowStart = () =>
        {
            users.Single().FailedPasswordAttemptWindowStart.ShouldEqual(user.FailedPasswordAttemptWindowStart);
        };
        private It should_return_user_with_same_FailedPasswordAnswerAttemptCount = () =>
        {
            users.Single().FailedPasswordAnswerAttemptCount.ShouldEqual(user.FailedPasswordAnswerAttemptCount);
        };
        private It should_return_user_with_same_FailedPasswordAnswerAttemptWindowStart = () =>
        {
            users.Single().FailedPasswordAnswerAttemptWindowStart.ShouldEqual(user.FailedPasswordAnswerAttemptWindowStart);
        };
        private It should_return_user_with_same_Roles = () =>
        {
            users.Single().Roles.ShouldContainOnly(user.Roles);
        };
        private It should_return_user_with_same_Values = () =>
        {
            users.Single().Values.ShouldContainOnly(user.Values);
        };

        private Cleanup all = () =>
        {
            mongo.DropUsers();
            mongo.DropRoles();
        };

        private static IMongoGateway mongo;
        private static string applicationName;
        private static int totalRecords;
        private static IEnumerable<User> users;
        private static User user;
    }
}
