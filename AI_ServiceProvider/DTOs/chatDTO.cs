using System.ComponentModel.DataAnnotations;

namespace AI_ServiceProvider.DTOs
{
    public class CreateChatRequestDto
    {
        [Required]
        public string Name { get; set; }
    }


    public class ChatResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
