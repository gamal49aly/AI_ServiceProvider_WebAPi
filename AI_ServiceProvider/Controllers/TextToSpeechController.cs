using AI_ServiceProvider.Controllers.Services;
using AI_ServiceProvider.Data;
using AI_ServiceProvider.DTOs;
using AI_ServiceProvider.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AI_ServiceProvider.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TextToSpeechController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITextToSpeechService _textToSpeechService;
        private readonly ILogger<TextToSpeechController> _logger;

        public TextToSpeechController(
            ApplicationDbContext context,
            ITextToSpeechService textToSpeechService,
            ILogger<TextToSpeechController> logger)
        {
            _context = context;
            _textToSpeechService = textToSpeechService;
            _logger = logger;
        }

        [HttpPost("synthesize")]
        public async Task<IActionResult> SynthesizeSpeech([FromBody] TextToSpeechRequestDto request)
        {
            try
            {
                // 1. Authenticate User
                var userIdNullable = GetUserId();
                if (userIdNullable == null) return Unauthorized();
                var userId = userIdNullable.Value;

                // 2. Validate Chat Ownership
                var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == request.ChatId && c.UserId == userId);
                if (chat == null) return NotFound("Chat not found or you do not have access.");

                // 3. Check usage credits
                if (!await HasUsageCredits(userId))
                {
                    return StatusCode(403, "You have reached your monthly usage limit.");
                }

                // 4. Call the Text-to-Speech Service
                _logger.LogInformation("Calling TTS service for user {UserId}", userId);
                var audioBytes = await _textToSpeechService.SynthesizeSpeechAsync(request.Text, request.Voice);
                _logger.LogInformation("Received {Size} bytes from TTS service", audioBytes.Length);

                // 5. Save the Results to the Database
                var input = new TextToSpeechInput
                {
                    ChatId = request.ChatId,
                    InputText = request.Text,
                    VoiceName = request.Voice,
                    AudioData = audioBytes  // ✅ Store the audio
                };
                _context.TextToSpeechInputs.Add(input);
                await _context.SaveChangesAsync();

                var output = new TextToSpeechOutput
                {
                    InputId = input.Id
                };
                _context.TextToSpeechOutputs.Add(output);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Saved TTS records to database with InputId {InputId}", input.Id);

                // 6. Return the MP3 audio file directly
                return File(audioBytes, "audio/mpeg", $"speech_{input.Id}.mp3");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "TTS service error");
                return StatusCode(500, new { error = "TTS service error", detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SynthesizeSpeech");
                return StatusCode(500, new { error = "An unexpected error occurred", detail = ex.Message });
            }
        }

        [HttpGet("voices")]
        public IActionResult GetAvailableVoices()
        {
            var voices = _textToSpeechService.GetAvailableVoices();
            return Ok(voices);
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetTextToSpeechChats()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            List<ChatResponseDto> chats = await _context.TextToSpeechInputs
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
        public async Task<IActionResult> GetTextToSpeechHistory(Guid chatId)
        {
            Guid? userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Verify chat ownership
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId && c.UserId == userId);
            if (chat == null) return NotFound("Chat not found or you do not have access.");

            List<TextToSpeechHistoryRowDTO> history = await _context.TextToSpeechInputs
                .Where(i => i.ChatId == chatId)
                .Include(i => i.Output)
                .OrderBy(i => i.CreatedAt)
                .Select(i => new TextToSpeechHistoryRowDTO
                {
                    InputId = i.Id,
                    InputText = i.InputText,
                    VoiceName = i.VoiceName,
                    CreatedAt = i.CreatedAt,
                    AudioData = i.AudioData, // Returns the audio bytes
                    GeneratedAt = i.Output != null ? i.Output.GeneratedAt : (DateTime?)null
                })
                .ToListAsync();

            return Ok(history);
        }

        private async Task<bool> HasUsageCredits(Guid userId)
        {
            var user = await _context.Users.Include(u => u.Subscription).FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.Subscription == null) return false;
            if (user.Subscription.MaxUsagePerMonth == -1) return true;

            var currentUsage = await _context.TextToSpeechInputs
                .CountAsync(i => i.Chat.UserId == userId &&
                                 i.CreatedAt.Year == DateTime.UtcNow.Year &&
                                 i.CreatedAt.Month == DateTime.UtcNow.Month);

            return currentUsage < user.Subscription.MaxUsagePerMonth;
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}