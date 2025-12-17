using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.Models
{
    public class TextToSpeechOutput
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid InputId { get; set; }

        // The URL to the generated audio file stored in Azure, Google Cloud, etc.
        [Required]
        [MaxLength(500)]
        public byte[] AudioData { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---

        [ForeignKey("InputId")]
        public virtual TextToSpeechInput Input { get; set; }
    
}
}
