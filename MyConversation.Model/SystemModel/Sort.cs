using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.SystemModel
{
    public class Sort<T>
    {
        public System.Linq.Expressions.Expression<Func<T, object>>? expression { set; get; }
        public bool IsAscent { set; get; }
    }
}
