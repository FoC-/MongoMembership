using System;
using System.Collections.Generic;
using System.Configuration;
using Machine.Specifications;
using MongoMembership.Mongo;

namespace MongoMembership.Tests
{
    internal class StubsBase
    {
        protected static User CreateUser(string applicationName)
        {
            return new User
            {
                ApplicationName = applicationName,
                Username = "UseRnAmE",
                Id = "user-Id",
                Comment = "comment",
                CreateDate = new DateTime(1920, 10, 10).ToUniversalTime(),
                Email = "em@a.il",
                FailedPasswordAnswerAttemptCount = 0,
                FailedPasswordAnswerAttemptWindowStart = new DateTime(1930, 10, 20).ToUniversalTime(),
                FailedPasswordAttemptCount = 1,
                FailedPasswordAttemptWindowStart = new DateTime(1930, 12, 12).ToUniversalTime(),
                IsAnonymous = false,
                IsApproved = true,
                IsDeleted = false,
                IsLockedOut = false,
                Values = new Dictionary<string, object>
                {
                    {"key1",1},
                    {"key2", "two"}
                },
                Roles = new List<string>
                {
                    "Role1","Role2","Role"
                }

            };
        }

        protected static IMongoGateway CreateMongoGateway()
        {
            var connectionString = ConfigurationManager.AppSettings["LOCALHOST_test"];
            return new MongoGateway(connectionString);
        }

        protected static Role CreateRole()
        {
            return new Role
            {
                ApplicationName = "test/app",
                RoleName = "AdMiN"
            };
        }

        Cleanup staff = () =>
        {
            IMongoGateway mongo = CreateMongoGateway();
            mongo.DropRoles();
            mongo.DropUsers();
        };
    }
}