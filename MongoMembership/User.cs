using System;
using System.Collections.Generic;

namespace MongoMembership
{
    internal class User
    {
        public string ApplicationName { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public string UsernameLowercase { get; set; }
        public string Email { get; set; }
        public string EmailLowercase { get; set; }
        public string Comment { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastLockedOutDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public DateTime FailedPasswordAttemptWindowStart { get; set; }
        public int FailedPasswordAnswerAttemptCount { get; set; }
        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }
        public List<string> Roles { get; set; }
        public Dictionary<string, object> Values { get; set; }

        public User()
        {
            this.Roles = new List<string>();
            this.Values = new Dictionary<string, object>();
        }

        public void AddValues(Dictionary<string, object> values)
        {
            foreach (var value in values)
            {
                if (this.Values.ContainsKey(value.Key))
                {
                    this.Values[value.Key] = value.Value;
                }
                else
                {
                    this.Values.Add(value.Key, value.Value);
                }
            }
        }

        public override string ToString()
        {
            return this.Username + " <" + this.Email + ">";
        }
    }
}
