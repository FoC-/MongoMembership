using MongoDB.Bson.Serialization.Attributes;

namespace MongoMembership
{
    [BsonIgnoreExtraElements]
    internal class Role
    {
        public string ApplicationName { get; set; }
        public string RoleName { get; set; }
        public string RoleNameLowercased { get; set; }
    }
}