using AI_ServiceProvider.Models;
using AI_ServiceProvider.Validators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.DTOs
{
    public class SpeechToTextRequestDTO
    {
        [Required]
        public Guid ChatId { get; set; }

        // The audio file from the client
        [Required]
        [AudioFile(maxFileSizeInMB:10)]
        public IFormFile AudioFile { get; set; }
    }

    public class SpeechToTextResponseDto
    {
        public Guid InputId { get; set; }
        public string TranscribedText { get; set; }
    }
}
