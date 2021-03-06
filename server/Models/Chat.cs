using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using PodNoms.Api.Services.Auth;

namespace PodNoms.Api.Models {
    public class ChatMessage : BaseEntity, IEntity {

        public ApplicationUser FromUser { get; set; }
        public ApplicationUser ToUser { get; set; }
        public string Message { get; set; }
        public DateTime? MessageSeen { get; set; }

    }
}
