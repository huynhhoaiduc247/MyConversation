using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Repository.Helper
{
    public class DbContext
    {
        private DbContext() { }
        private static DbContext instance = null;
        public IMongoDatabase _db;
        public MongoClient mgClient;
        public static DbContext Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DbContext();
                }
                return instance;
            }
        }
        public void Init(string connectionString)
        {
            mgClient = new MongoClient(connectionString);
        }

        public IMongoDatabase GetDB(string dbName)
        {
            return mgClient.GetDatabase(dbName);
        }
    }
}
