using MyConversation.Model.Common;
using MyConversation.Model.Model;
using MyConversation.Model.SystemModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.ModelParam
{
    public class UserCustomModel: BaseModel
    {
        [Required]
        public string Name { set; get; } = string.Empty;
        public string Phone { set; get; } = string.Empty;
        public string Address { set; get; } = string.Empty;
        [Required]
        public string Username { set; get; } = string.Empty;
        [Required]
        public string Password { set; get; } = string.Empty;
        public DateTime DOB { set; get; }
        public bool IsAdmin { set; get; }
        public EnumDefinition.UserStatus Status { set; get; }
        public bool IsRoot { set; get; }
        public string? CurrentToken { set; get; }
        public string? ConversationId { set; get; }
        public Conversation? Conversation { set; get; }
    }
}
