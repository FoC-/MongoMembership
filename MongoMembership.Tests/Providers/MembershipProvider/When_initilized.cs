using System.Configuration;
using FluentAssertions;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests.Providers.MembershipProvider
{
    [Subject(typeof(MongoMembershipProvider))]
    internal class When_initilized : ProvidersStubs
    {
        Establish context = () =>
        {
            provider = CreateMembershipProvider();
        };

        Because of = () =>
        {
            connectionStringFromConfig = ConfigurationManager.AppSettings.Get("LOCALHOST_test");
        };

        It should_return_connection_string_from_config_file = () =>
            provider.MongoConnectionString.Should().BeEquivalentTo(connectionStringFromConfig);

        private static MongoMembershipProvider provider;
        private static string connectionStringFromConfig;
    }
}