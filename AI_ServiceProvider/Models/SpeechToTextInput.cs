using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.Models
{
    public class SpeechToTextInput
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ChatId { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // This will store the raw audio data from the uploaded file
        [MaxLength(20 * 1024 * 1024)] // Max 20MB
        public byte[] AudioData { get; set; }

        // Optional: Store original filename and metadata
        [MaxLength(255)]
        public string? OriginalFileName { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }

        public virtual SpeechToTextOutput? Output { get; set; }
    }
    }
