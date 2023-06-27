using MyConversation.Model.Common;
using MyConversation.Model.SystemModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.Model
{
    public class Conversation: BaseModel
    {
        public string Name { set; get; }
        public EnumDefinition.ConversationType ConversationType { set; get; }
        public List<string> Attendance { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime LastActionDate { set; get; }
        public string? StartBy { set; get; }
        public string? LastActionBy { set; get; }
        public string? LastMessage { set; get; }
        public List<string> UserRead { set; get; } = new List<string>();
    }
}
