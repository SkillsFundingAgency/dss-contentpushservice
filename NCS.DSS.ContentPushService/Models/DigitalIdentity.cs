﻿using System;

namespace NCS.DSS.ContentPushService.Models
{
    public class DigitalIdentity
    {
        //generic properties
        public bool? CreateDigitalIdentity { get; set; }
        public bool? DeleteDigitalIdentity { get; set; }
        public bool? ChangeEmailAddress { get; set; }
        public bool? UpdateDigitalIdentity { get; set; }

        //Create account properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public Guid? CustomerGuid { get; set; }

        //delete properties
        public Guid? IdentityStoreId { get; set; }

        //change email address
        public string NewEmail { get; set; }
        public string CurrentEmail { get; set; }

    }
}
