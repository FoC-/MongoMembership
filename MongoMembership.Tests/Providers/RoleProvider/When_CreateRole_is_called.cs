using System;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_CreateRole_is_called:StubsBase
    {
        private Establish conext = () =>
        {
            roleName = "AdmiN";
            provider = CreateRoleProvider();
        };

        private Because of = () =>
            exception = Catch.Exception(() => provider.CreateRole(roleName));

        private It should_not_throw_exception = () =>
            exception.ShouldBeNull();

        private Cleanup staff = () =>
            provider.DeleteRole(roleName, false);

        private static Exception exception;
        private static MongoRoleProvider provider;
        private static string roleName;
    }
}