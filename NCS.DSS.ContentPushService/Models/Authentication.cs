namespace NCS.DSS.ContentPushService.Models
{
    public class Authentication
    {
        public required string AuthorityUri { get; set; }
        public required string PushServiceClientId { get; set; }
        public required string PushServiceClientSecret { get; set; }
        public required string Tenant { get; set; }
    }
}
