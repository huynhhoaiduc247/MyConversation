using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.SystemModel
{
    public abstract class BaseModel
    {
        public string Id { set; get; }
        public string ClientId { set; get; }
        public bool Active { set; get; }
        public DateTime CreatedDate { set; get; }
        public DateTime ModifiedDate { set; get; }
        public string CreatedBy { set; get; }
        public string ModifiedBy { set; get; }

        public BaseModel()
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            CreatedBy = string.Empty;
            ModifiedBy = string.Empty;
            ClientId = string.Empty;
        }
    }
}
