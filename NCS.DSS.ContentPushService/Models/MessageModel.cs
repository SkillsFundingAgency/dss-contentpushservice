using System;

namespace NCS.DSS.ContentPushService.Models
{
    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public Uri URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
        public bool? DataCollections { get; set; }
    }
}
