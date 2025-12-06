using System.ComponentModel.DataAnnotations;

namespace AI_ServiceProvider.DTOs
{
    public class CreateChatRequestDTO
    {
        [Required]
        public string Name { get; set; }
    }

    public class ChatResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
