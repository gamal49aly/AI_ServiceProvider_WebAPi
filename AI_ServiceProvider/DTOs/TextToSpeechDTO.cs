using System.ComponentModel.DataAnnotations;

namespace AI_ServiceProvider.DTOs
{

    public class TextToSpeechRequestDto
    {
        [Required]
        public string Text { get; set; }

        [Required]
        public Guid ChatId { get; set; }

        // Optional: JSON string for voice settings like {"voiceName": "en-US-JennyNeural", "language": "en-US"}
        public string? VoiceSettings { get; set; } = "{\"voiceName\": \"en-US-JennyNeural\"}";
    }
    public class TextToSpeechResponseDTO
    {

        public Guid InputId { get; set; }
        public string AudioDataBase64 { get; set; }
    }

}
