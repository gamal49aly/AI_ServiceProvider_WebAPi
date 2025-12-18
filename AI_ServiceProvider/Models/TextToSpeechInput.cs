using AI_ServiceProvider.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TextToSpeechInput
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ChatId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // The text the user wanted to synthesize
    [Required]
    [MaxLength(5000)]
    public string InputText { get; set; }

    // The voice name used for synthesis
    [MaxLength(100)]
    public string VoiceName { get; set; }

    // Store the generated audio file directly in the database
    [MaxLength(1073741824)] // Max 1GB
    public byte[] AudioData { get; set; }

    // --- Navigation Properties ---

    [ForeignKey("ChatId")]
    public virtual Chat Chat { get; set; }

    // One-to-one relationship with the output
    public virtual TextToSpeechOutput? Output { get; set; }
}