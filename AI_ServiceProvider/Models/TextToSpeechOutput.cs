using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TextToSpeechOutput
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid InputId { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---

    [ForeignKey("InputId")]
    public virtual TextToSpeechInput Input { get; set; }
}