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
    public class TextToSpeechController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITextToSpeechService _textToSpeechService;

        public TextToSpeechController(ApplicationDbContext context, ITextToSpeechService textToSpeechService)
        {
            _context = context;
            _textToSpeechService = textToSpeechService;
        }

        [HttpPost("synthesize")]
        public async Task<IActionResult> SynthesizeText([FromBody] TextToSpeechRequestDto request)
        {
            // 1. Authenticate User
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // 2. Validate Chat Ownership
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == request.ChatId && c.UserId == userId);
            if (chat == null) return NotFound("Chat not found or you do not have access.");

            // 3. Check Subscription Limits
            if (!await HasUsageCredits(userId.Value))
            {
                return Forbid("You have reached your monthly usage limit.");
            }

            // 4. Call the Text-to-Speech Service
            var audioBytes = await _textToSpeechService.ConvertTextToSpeechAsync(request.Text, request.VoiceSettings);

            // 5. Save the Results to the Database
            var input = new TextToSpeechInput
            {
                ChatId = request.ChatId,
                InputText = request.Text,
                VoiceSettings = request.VoiceSettings
            };
            _context.TextToSpeechInputs.Add(input);
            await _context.SaveChangesAsync(); // Save first to get the InputId

            var output = new TextToSpeechOutput
            {
                InputId = input.Id,
                AudioData = audioBytes // Store the raw byte array from the service
            };
            _context.TextToSpeechOutputs.Add(output);
            await _context.SaveChangesAsync(); // Save the output

            // 6. Prepare and Return the Response
            var audioDataBase64 = Convert.ToBase64String(audioBytes);
            return Ok(new TextToSpeechResponseDTO { InputId = input.Id, AudioDataBase64 = audioDataBase64 });
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

            // Count usage from BOTH ImageParser and Text-to-Speech for the current month
            var imageUsage = await _context.ImageParserInputs
                .CountAsync(i => i.Chat.UserId == userId &&
                                 i.UploadedAt.Year == DateTime.UtcNow.Year &&
                                 i.UploadedAt.Month == DateTime.UtcNow.Month);

            var textToSpeechUsage = await _context.TextToSpeechInputs
                .CountAsync(t => t.Chat.UserId == userId && // Note the navigation path
                                 t.CreatedAt.Year == DateTime.UtcNow.Year &&
                                 t.CreatedAt.Month == DateTime.UtcNow.Month);

            var totalUsage = imageUsage + textToSpeechUsage;

            return totalUsage < user.Subscription.MaxUsagePerMonth;
        }
    }
}