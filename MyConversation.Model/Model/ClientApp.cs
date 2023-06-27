using MyConversation.Model.SystemModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.Model
{
    public class ClientApp: BaseModel
    {
        [Required]
        public string Name { set; get; }
        public string Description { set; get; }
        [Required]
        public int NumberOfUser { set; get; }
        [Required]
        public string UserAdmin { set; get; }
        [Required]
        public string InitPassword { set; get; }
    }
}
