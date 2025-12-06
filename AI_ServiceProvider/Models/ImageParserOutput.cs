using AI_ServiceProvider.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.Models
{
    public class ImageParserOutput
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid InputId { get; set; }

        public string ParsedData { get; set; } // The JSON result from the AI model

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("InputId")]
        public virtual ImageParserInput Input { get; set; }
    }
}