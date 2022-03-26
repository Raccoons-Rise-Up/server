using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameServer.Server.MongoDb
{
    public class PlayerModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
