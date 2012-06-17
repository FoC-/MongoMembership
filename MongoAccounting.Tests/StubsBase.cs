using System;
using MongoAccounting.Mongo;

namespace MongoAccounting.Tests
{
    public class StubsBase
    {
        public static User CreateUser(string applicationName)
        {
            return new User
            {
                ApplicationName = applicationName,
                Comment = "comment",
                CreateDate = new DateTime(1920, 10, 10),
                Email = "em@a.il",
                FailedPasswordAnswerAttemptCount = 0,
                FailedPasswordAnswerAttemptWindowStart = new DateTime(1930, 10, 20),
                FailedPasswordAttemptCount = 1,
                FailedPasswordAttemptWindowStart = new DateTime(1930, 12, 12),
                IsAnonymous = false,
                IsApproved = true,
                IsDeleted = false,
                IsLockedOut = false,
            };
        }

        public static IMongoGateway CreateMongoGateway()
        {
            return new MongoGateway("mongodb://localhost/TestStorage");
        }
    }
}