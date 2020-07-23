using System;
using System.Collections.Generic;
using System.Text;

namespace NCS.DSS.ContentPushService.Models
{
    public class DigitalIdentity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public Guid CustomerId { get; set; }

    }
}
