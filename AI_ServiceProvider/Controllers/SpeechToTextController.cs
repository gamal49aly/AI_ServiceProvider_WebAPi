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
                return Forbid("You have reached your monthly usage limit.");
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