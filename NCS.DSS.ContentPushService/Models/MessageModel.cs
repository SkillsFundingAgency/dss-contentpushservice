using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Models
{
    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastmodifiedDate { get; set; }
        public string URL { get; set; }
    }
}
