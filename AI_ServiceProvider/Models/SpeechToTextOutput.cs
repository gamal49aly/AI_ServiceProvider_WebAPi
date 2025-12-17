using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.Models
{
    public class SpeechToTextOutput
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid InputId { get; set; }

        // The transcribed text from the AI model
        [Required]
        public string TranscribedText { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---

        [ForeignKey("InputId")]
        public virtual SpeechToTextInput Input { get; set; }
    }
}
