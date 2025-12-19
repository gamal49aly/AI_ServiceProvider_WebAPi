namespace AI_ServiceProvider.DTOs
{
    public class ImageParserHistoryRow
    {
        public Guid InputId { set; get; }
        public string ImageUrl {  set; get; }
        public string? JsonSchema {  set; get; }
        public DateTime UploadedAt {  set; get; }
        public string? ParsedData {  set; get; }
        public DateTime? GeneratedAt {  set; get; }
    }
}
