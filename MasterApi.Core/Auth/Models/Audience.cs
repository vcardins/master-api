using System;
using System.Collections.Generic;
using MasterApi.Core.Auth.Enums;
using MasterApi.Core.Models;

namespace MasterApi.Core.Auth.Models
{
    public class Audience : BaseObjectState
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string Name { get; set; }
        public ApplicationTypes ApplicationType { get; set; }
        public bool Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }

        public Audience()
        {
            ClientId = Guid.NewGuid().ToString();
        }
    }
}
