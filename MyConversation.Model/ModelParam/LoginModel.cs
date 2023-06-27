using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.ModelParam
{
    public class LoginModel
    {
        [Required]
        public string Username { set; get; }
        [Required]
        public string Password { set; get; }
    }
}
