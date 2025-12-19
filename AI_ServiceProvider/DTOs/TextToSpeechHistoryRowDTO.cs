namespace AI_ServiceProvider.DTOs
{
    public class TextToSpeechHistoryRowDTO
    {
        public Guid InputId { get; set; }
        public string? InputText { get; set; }
        public string? VoiceName { get; set; }
        public DateTime CreatedAt { get; set; }

        public byte[]? AudioData { get; set; }

        public DateTime? GeneratedAt { get; set; }
    }
}
