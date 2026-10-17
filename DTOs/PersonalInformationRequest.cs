using System.ComponentModel.DataAnnotations;

namespace EverestRemit.Api.DTOs
{
    public sealed class PersonalInformationRequest
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = "";

        public DateOnly DateOfBirth { get; set; }

        [Required, MaxLength(100)]
        public string Nationality { get; set; } = "";

        [MaxLength(500)]
        public string? Address { get; set; }
    }
}
