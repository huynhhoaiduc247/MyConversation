using MyConversation.Model.SystemModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.Model
{
    public class UserToken: BaseModel
    {
        [Required]
        public string Token { set; get; }
        [Required]
        public string Username { set; get; }
    }
}
