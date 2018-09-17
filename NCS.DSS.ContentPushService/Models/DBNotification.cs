using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Models
{
    public class DBNotification
    {
        public string MessageId { get; set; }
        public int HttpCode { get; set; }
        public Models.Notification Notification { get; set; }
        public string AppIdUri { get; set; }
        public string ClientUrl { get; set; }
        public string BearerToken { get; set; }
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
