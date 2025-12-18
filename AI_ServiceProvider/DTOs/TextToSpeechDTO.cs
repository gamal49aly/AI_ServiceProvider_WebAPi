using System.ComponentModel.DataAnnotations;

public class TextToSpeechRequestDto
{
    [Required]
    public Guid ChatId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; }

    [Required]
    public string Voice { get; set; }
}