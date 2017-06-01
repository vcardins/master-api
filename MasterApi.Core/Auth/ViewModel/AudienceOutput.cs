
using MasterApi.Core.Auth.Enums;

namespace MasterApi.Core.Auth.ViewModel
{
    public class AudienceOutput
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
        public ApplicationTypes ApplicationType { get; set; }
        public bool Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }
        public string AdminEmail { get; set; }
    }
}
