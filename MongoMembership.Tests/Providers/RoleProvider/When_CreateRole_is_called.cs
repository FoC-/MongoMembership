using System;
using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.RoleProvider
{
    [Subject(typeof(MongoRoleProvider))]
    internal class When_CreateRole_is_called : ProvidersStubs
    {
        Establish conext = () =>
        {
            roleName = "AdmiN";
            provider = CreateRoleProvider();
        };

        Because of = () =>
            exception = Catch.Exception(() => provider.CreateRole(roleName));

        It should_not_throw_exception = () =>
            exception.Should().BeNull();

        private static Exception exception;
        private static MongoRoleProvider provider;
        private static string roleName;
    }
}