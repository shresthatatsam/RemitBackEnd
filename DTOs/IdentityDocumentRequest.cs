using System.ComponentModel.DataAnnotations;

namespace EverestRemit.Api.DTOs
{
    public class IdentityDocumentRequest
    {
        [Required, MaxLength(50)]
        public string DocumentType { get; set; } = "";

        [Required, MaxLength(100)]
        public string DocumentNumber { get; set; } = "";

        [Required]
        public IFormFile DocumentFront { get; set; } = null!;

        // Optional for document types that have no reverse side.
        public IFormFile? DocumentBack { get; set; }
    }
}
