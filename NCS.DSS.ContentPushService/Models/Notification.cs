using System;

namespace NCS.DSS.ContentPushService.Models
{
    public class Notification
    {
        public Guid CustomerId { get; set; }
        public Uri ResourceURL { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}