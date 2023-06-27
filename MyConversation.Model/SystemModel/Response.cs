using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConversation.Model.SystemModel
{
    public class Response<T>
    {
        public string? Status { set; get; }
        public bool IsSuccess { set; get; }
        public T? Data { set; get; }
        public string? Message { set; get; }
    }
}
