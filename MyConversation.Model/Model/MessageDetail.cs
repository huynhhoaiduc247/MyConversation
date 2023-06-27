using MyConversation.Model.SystemModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.Model
{
    public class MessageDetail: BaseModel
    {
        public string ConversationId { set; get; }
        public string Content { set; get; }
        public bool IsEdit { set; get; }
        public List<string> UserRead { set; get; }
        public string From { set; get; }
        public List<string> To { set; get; }
    }
}
