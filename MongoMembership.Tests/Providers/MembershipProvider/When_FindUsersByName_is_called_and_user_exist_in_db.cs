using System;
using System.Web.Security;
using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.MembershipProvider
{
    [Subject(typeof(MongoMembershipProvider))]
    internal class When_FindUsersByName_is_called_and_user_exist_in_db : ProvidersStubs
    {
        Establish context = () =>
        {
            provider = CreateMembershipProvider();
            userName = "UserName";

            MembershipCreateStatus status;
            provider.CreateUser(userName, "aqswdefr1", "email@email.com", null, null, true, Guid.NewGuid(), out status);
        };

        Because of = () =>
        {
            int totalRecords;
            users = provider.FindUsersByName(userName, 0, 10, out totalRecords);
        };

        It should_return_user = () =>
            users.Should().NotBeNull();

        private static MembershipUserCollection users;
        private static MongoMembershipProvider provider;
        private static string userName;
    }
}