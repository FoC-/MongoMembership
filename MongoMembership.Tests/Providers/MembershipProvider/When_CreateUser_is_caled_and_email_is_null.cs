using System.Web.Security;
using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.MembershipProvider
{
    [Subject(typeof(MongoMembershipProvider))]
    internal class When_CreateUser_is_caled_and_email_is_null : ProvidersStubs
    {
        Establish context = () =>
        {
            username = "name";
            provider = CreateMembershipProvider();
        };

        Because of = () =>
        {
            MembershipCreateStatus status;
            user = provider.CreateUser(username, "pass", null, null, null, true, "dfg1dr3", out status);
        };

        It should_return_user = () =>
            user.Should().NotBeNull();

        It should_return_user_with_same_name = () =>
            user.UserName.Should().BeEquivalentTo(username);

        private static MembershipUser user;
        private static MongoMembershipProvider provider;
        private static string username;
    }
}