using System.ComponentModel.DataAnnotations;

namespace AI_ServiceProvider.DTOs
{
    public class GoogleLoginRequestDTO
    {
        [Required]
        public string IdToken { get; set; }
    }
}
