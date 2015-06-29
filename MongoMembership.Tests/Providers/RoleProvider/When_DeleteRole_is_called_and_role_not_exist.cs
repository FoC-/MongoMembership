using System;
using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_DeleteRole_is_called_and_role_not_exist : ProvidersStubs
    {
        Establish conext = () =>
        {
            provider = CreateRoleProvider();
        };

        Because of = () =>
            exception = Catch.Exception(() => provider.DeleteRole("NotAdmiN", false));

        It should_not_fail = () =>
            exception.Should().BeNull();

        private static MongoRoleProvider provider;
        private static Exception exception;
    }
}