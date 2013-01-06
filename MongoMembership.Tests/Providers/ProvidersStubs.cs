using System.Web.Security;
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
    }
}