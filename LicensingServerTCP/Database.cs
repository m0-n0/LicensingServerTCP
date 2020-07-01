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
        private static MongoDB.Driver.MongoClient _client;
        private static MongoDB.Driver.IMongoDatabase _database;
        private static MongoDB.Driver.IMongoCollection<LicensingServerTCP.models.Users> _collection;
        static Database()
        {
            string connectionString = "mongodb://localhost";
            _client = new MongoDB.Driver.MongoClient(connectionString);
            _database = _client.GetDatabase("LicensePanel");
            _collection = _database.GetCollection<LicensingServerTCP.models.Users>("users");
        }

        public LicensingServerTCP.models.Users GetUserByHwid(string hwid)
        {
            var filter = new MongoDB.Bson.BsonDocument { { "hwid", hwid } };
            var def = _collection.Find(filter).FirstOrDefault();
            return def;
        }
        public void CreateUserByHwid(string hwid, string ip, string info = "")
        {
            _collection.InsertOne(new LicensingServerTCP.models.Users
            {
                hwid = hwid,
                userIp = ip,
                info = info,
                Active = false,
                ChangedIp = false,

            });
            //return GetUserByHwid(hwid);
        }
        public LicensingServerTCP.models.Users Update(LicensingServerTCP.models.Users us)
        {
            _collection.FindOneAndUpdate(
                new BsonDocumentFilterDefinition<LicensingServerTCP.models.Users>(
                    new BsonDocument { { "_id", us._id } }),
                new ObjectUpdateDefinition<LicensingServerTCP.models.Users>(us));

            return GetUserByHwid(us.hwid);
        }

    }
}
