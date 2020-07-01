using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    class Database
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<User> _collection;

        public Database()
        {
            string connectionString = "mongodb://localhost";
            _client = new MongoDB.Driver.MongoClient(connectionString);
            _database = _client.GetDatabase("LicensePanel");
            _collection = _database.GetCollection<User>("users");
        }

        public User GetUserByHwid(string hwid)
        {
            var filter = new BsonDocument { { "hwid", hwid } };
            var def = _collection.Find(filter).FirstOrDefault();
            return def;
        }
        public void CreateUserByHwid(string hwid, string ip, string info = "")
        {
            _collection.InsertOne(new User
            {
                hwid = hwid,
                userIp = ip,
                info = info,
                Active = false,
                ChangedIp = false,

            });
        }
        public User Update(User us)
        {
            _collection.FindOneAndUpdate(
                new BsonDocumentFilterDefinition<User>(
                    new BsonDocument { { "_id", us._id } }),
                new ObjectUpdateDefinition<User>(us));

            return GetUserByHwid(us.hwid);
        }

    }
}
