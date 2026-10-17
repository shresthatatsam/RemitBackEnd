using System.ComponentModel.DataAnnotations;

namespace EverestRemit.Api.DTOs
{
    public sealed class SelfieRequest
    {
        [Required]
        public IFormFile Selfie { get; set; } = null!;
    }
}
