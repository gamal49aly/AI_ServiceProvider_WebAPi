using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.Models
{
    public class TextToSpeechInput
    {

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ChatId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(5000)] 
        public string InputText { get; set; }

        // Optional: Store voice settings (e.g., voice name, language, speed) as a JSON string
        [MaxLength(500)]
        public string? VoiceSettings { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }

        // One-to-one relationship with the output
        public virtual TextToSpeechOutput? Output { get; set; }

    }
}
