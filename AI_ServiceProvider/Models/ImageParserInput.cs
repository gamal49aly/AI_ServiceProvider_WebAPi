using AI_ServiceProvider.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.Models
{
    public class ImageParserInput
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ChatId { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } // URL from Google Drive for history

        public string? JsonSchema { get; set; } // The keys the user wanted to extract

        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }

        public virtual ImageParserOutput? Output { get; set; }
    }
}