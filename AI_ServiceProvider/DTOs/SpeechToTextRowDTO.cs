namespace AI_ServiceProvider.DTOs
{
    public class SpeechToTextRowDTO
    {

        public Guid InputId {  get; set; }
        public string? OriginalFileName {  get; set; }
        public byte[]? AudioData {  get; set; }
        public DateTime UploadedAt {  get; set; }
        public string? TranscribedText {  get; set; }
        public DateTime? GeneratedAt {  get; set; }
            
    }

}

