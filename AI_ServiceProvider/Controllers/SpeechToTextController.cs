// In Controllers/SpeechToTextController.cs

using AI_ServiceProvider.Data;
using AI_ServiceProvider.DTOs;
using AI_ServiceProvider.Models;
using AI_ServiceProvider.Controllers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace AI_ServiceProvider.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SpeechToTextController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpeechToTextService _speechToTextService;

        public SpeechToTextController(ApplicationDbContext context, ISpeechToTextService speechToTextService)
        {
            _context = context;
            _speechToTextService = speechToTextService;
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> TranscribeAudio([FromForm] SpeechToTextRequestDTO request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Validate Chat Ownership
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == request.ChatId && c.UserId == userId);
            if (chat == null) return NotFound("Chat not found or you do not have access.");

            // Check Subscription Limits
            if (!await HasUsageCredits(userId.Value))
            {
                return StatusCode(403, "You have reached your monthly usage limit.");
            }

            // Read the uploaded audio file into a byte array
            byte[] audioBytes;
            using (var stream = request.AudioFile.OpenReadStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    audioBytes = memoryStream.ToArray();
                }
            }

            // Call the Speech-to-Text Service
            string transcribedText = await _speechToTextService.TranscribeAudioAsync(
                new MemoryStream(audioBytes), // Create a new stream from the byte array
                request.AudioFile.ContentType
            );

            // Save the Results to the Database
            var input = new SpeechToTextInput
            {
                ChatId = request.ChatId,
                AudioData = audioBytes, // Save the raw byte array directly
                OriginalFileName = request.AudioFile.FileName
            };
            _context.SpeechToTextInputs.Add(input);
            await _context.SaveChangesAsync(); // Save first to get the InputId

            var output = new SpeechToTextOutput
            {
                InputId = input.Id,
                TranscribedText = transcribedText
            };
            _context.SpeechToTextOutputs.Add(output);
            await _context.SaveChangesAsync(); // Save the output

            return Ok(new SpeechToTextResponseDto { InputId = input.Id, TranscribedText = output.TranscribedText });
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetSpeechToTextChats()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            List<ChatResponseDto> chats = await _context.SpeechToTextInputs
                .Where(i => i.Chat.UserId == userId)
                .Select(i => new ChatResponseDto
                {
                    Id = i.ChatId,
                    Name = i.Chat.Name,
                    CreatedAt = i.Chat.CreatedAt
                })
                .Distinct()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(chats);
        }

        [HttpGet("history/{chatId}")]
        public async Task<IActionResult> GetSpeechToTextHistory(Guid chatId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Verify chat ownership
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId && c.UserId == userId);
            if (chat == null) return NotFound("Chat not found or you do not have access.");

            List<SpeechToTextRowDTO> history = await _context.SpeechToTextInputs
                .Where(i => i.ChatId == chatId)
                .Include(i => i.Output)
                .OrderBy(i => i.UploadedAt)
                .Select(i => new SpeechToTextRowDTO
                {
                    InputId = i.Id,
                    OriginalFileName = i.OriginalFileName,
                    AudioData = i.AudioData, // Returns the audio bytes
                    UploadedAt = i.UploadedAt,
                    TranscribedText = i.Output != null ? i.Output.TranscribedText : null,
                    GeneratedAt = i.Output != null ? i.Output.GeneratedAt : (DateTime?)null
                })
                .ToListAsync();

            return Ok(history);
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private async Task<bool> HasUsageCredits(Guid userId)
        {
            var user = await _context.Users.Include(u => u.Subscription).FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.Subscription == null) return false;

            if (user.Subscription.MaxUsagePerMonth == -1) return true; // Unlimited

            // Count usage from Image Parser, Text-to-Speech, and Speech-to-Text
            var imageUsage = await _context.ImageParserInputs
                .CountAsync(i => i.Chat.UserId == userId &&
                                 i.UploadedAt.Year == DateTime.UtcNow.Year &&
                                 i.UploadedAt.Month == DateTime.UtcNow.Month);

            var textToSpeechUsage = await _context.TextToSpeechInputs
                .CountAsync(t => t.Chat.UserId == userId &&
                                 t.CreatedAt.Year == DateTime.UtcNow.Year &&
                                 t.CreatedAt.Month == DateTime.UtcNow.Month);

            var speechToTextUsage = await _context.SpeechToTextInputs
                .CountAsync(s => s.Chat.UserId == userId &&
                                 s.UploadedAt.Year == DateTime.UtcNow.Year &&
                                 s.UploadedAt.Month == DateTime.UtcNow.Month);

            var totalUsage = imageUsage + textToSpeechUsage + speechToTextUsage;

            return totalUsage < user.Subscription.MaxUsagePerMonth;
        }
    }
}