using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicensingServerTCP.models
{
    class Users
    {

        public MongoDB.Bson.ObjectId _id { get; set; }
        public int __v { get; set; }
        public string hwid { get; set; }
        public string userIp { get; set; }
        public string info { get; set; }
        public List<string> auth { get; set; }
        public bool Active { get; set; }
        public bool ChangedIp { get; set; }

    }
}
