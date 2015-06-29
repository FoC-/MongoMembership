using System.Web.Security;
using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers
{
    internal class ProvidersStubs : StubsBase
    {
        protected static MongoMembershipProvider CreateMembershipProvider()
        {
            return (MongoMembershipProvider)Membership.Provider;
        }

        protected static MongoRoleProvider CreateRoleProvider()
        {
            return (MongoRoleProvider)Roles.Provider;
        }

        protected static void AddUser(MongoMembershipProvider membershipProvider, string username)
        {
            MembershipCreateStatus status;
            membershipProvider.CreateUser(username, "password123", username + "@em.ail", null, null, true, "Id-" + username, out status);
            status.Should().Be(MembershipCreateStatus.Success);
        }
    }
}