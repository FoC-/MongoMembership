using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using MongoAccounting.Mongo;
using MongoAccounting.Providers;

namespace MongoAccounting.Tests
{
    internal class StubsBase
    {
        public static User CreateUser(string applicationName)
        {
            return new User
            {
                ApplicationName = applicationName,
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

        public static IMongoGateway CreateMongoGateway()
        {
            return new MongoGateway("mongodb://localhost/TestMongoGateway");
        }

        public static MongoMembershipProvider CreateProvider()
        {
            return new MongoMembershipProvider();
        }

        public static NameValueCollection CreateConfig()
        {
            var config = new NameValueCollection();
            config.Add("connectionStringKeys", "LOCALHOST_test ,MONGOLAB_URI, ;LOCALHOST");
            return config;
        }
    }
}