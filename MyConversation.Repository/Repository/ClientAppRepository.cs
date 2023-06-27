using MyConversation.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Repository.Repository
{
    public class ClientAppRepository : MongoRepository<ClientApp>
    {
        public ClientAppRepository(string _dbName) : base(_dbName) { }
    }
}
