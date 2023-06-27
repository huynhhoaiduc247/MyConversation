using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.Common
{
    public static class EnumDefinition
    {
        public enum UserStatus  
        {
            offline,
            online
        }
        public enum ConversationType
        {
            normal,
            group
        }
    }
}
