using System.Web.Security;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.MembershipProvider
{
    [Subject(typeof(MongoMembershipProvider))]
    class When_CreateUser_is_caled_and_email_is_null : StubsBase
    {
        Establish context = () =>
        {
            username = "name";
            provider = CreateProvider();
        };

        Because of = () =>
        {
            MembershipCreateStatus status;
            user = provider.CreateUser(username, "pass", null, null, null, true, "dfg1dr3", out status);
        };

        It should_return_user = () =>
        {
            user.ShouldNotBeNull();
        };

        It should_return_user_with_same_name = () =>
        {
            user.UserName.ShouldEqual(username);
        };

        Cleanup staff = () =>
        {
            provider.DeleteUser(username, true);
        };

        private static MembershipUser user;
        private static MongoMembershipProvider provider;
        private static string username;
    }
}