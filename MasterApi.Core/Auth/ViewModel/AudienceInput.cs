using System.ComponentModel.DataAnnotations;
using MasterApi.Core.Auth.Enums;

namespace MasterApi.Core.Auth.ViewModel
{
    public class AudienceInput
    {
        [Required]
        [MaxLength(50)]
        public string ClientId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public ApplicationTypes ApplicationType { get; set; }
        public bool Active { get; set; }
        [Required]
        public int RefreshTokenLifeTime { get; set; }
        [Required]
        public string AllowedOrigin { get; set; }
        public string AdminEmail { get; set; }
    }
}
