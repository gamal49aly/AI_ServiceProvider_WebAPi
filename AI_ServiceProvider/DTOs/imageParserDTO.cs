using System.ComponentModel.DataAnnotations;

namespace AI_ServiceProvider.DTOs
{
    public class imageParserDTO
    {
    }
    public class ParseImageRequestDto
    {
        [Required]
        public Guid ChatId { get; set; }

        [Required]
        public IFormFile Image { get; set; }

        [Required]
        public string JsonKeys { get; set; } 
    }

    public class ParseImageResponseDto
    {
        public Guid InputId { get; set; }
        public string ParsedData { get; set; }    // The final JSON result from the AI
    }
}
