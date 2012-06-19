using System.Configuration;
using Machine.Specifications;
using MongoMembership.Providers;

namespace MongoMembership.Tests
{
    [Subject(typeof(MongoMembershipProvider))]
    internal class When_initilized : StubsBase
    {
        private Establish context = () =>
        {
            connectionStringFromConfig = ConfigurationManager.AppSettings.Get("LOCALHOST_test");
            provider = CreateProvider();
        };

        private Because of = () =>
        {
            provider.Initialize("null", CreateConfig());
        };

        private It should_return_connection_string_from_config_file = () =>
        {
            provider.MongoConnectionString.ShouldEqual(connectionStringFromConfig);
        };

        private static MongoMembershipProvider provider;
        private static string connectionStringFromConfig;
    }
}