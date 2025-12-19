
using AI_ServiceProvider.Data;
using AI_ServiceProvider.DTOs;
using AI_ServiceProvider.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using AI_ServiceProvider.Controllers.Services;



namespace AI_ServiceProvider.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImageParserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //private readonly IGoogleDriveService _googleDriveService;
        private readonly IImageParsingService _imageParsingService; // Service for AI API 
        private readonly IConfiguration _configuration;
        public ImageParserController(ApplicationDbContext context, IImageParsingService imageParsingService, IConfiguration configuration)
        {
            _context = context;
            _imageParsingService = imageParsingService;
            _configuration = configuration;
        }

        [HttpPost("parse")]
        public async Task<IActionResult> ParseImage([FromForm] ParseImageRequestDto request)
        {

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            //  Validate Chat Ownership
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == request.ChatId && c.UserId == userId);
            if (chat == null) return NotFound("Chat not found or you do not have access.");

            //  Check Subscription Limits
            if (!await HasUsageCredits(userId.Value))
            {
                return StatusCode(403, "You have reached your monthly usage limit.");
            }

            //  Send Image Bytes to AI Model for Parsing
            string aiResponse;
            using (var stream = request.Image.OpenReadStream())
            {
                // Pass the content type from the IFormFile to the service
                aiResponse = await _imageParsingService.ParseImageAsync(stream, request.Image.ContentType, request.JsonKeys);
            }

            // Upload Image to Google Drive for Chat History
            //var folderId = _configuration["GoogleDriveSettings:FolderId"]; // Get from config
            //var imageUrl = await _googleDriveService.UploadFileAsync(request.Image, folderId);
            //if (string.IsNullOrEmpty(imageUrl))
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save image for history.");
            //}

            ////Save Results to Database
            //var input = new ImageParserInput
            //{
            //    ChatId = request.ChatId,
            //    ImageUrl = imageUrl,
            //    JsonSchema = request.JsonKeys
            //};
            //_context.ImageParserInputs.Add(input);
            //await _context.SaveChangesAsync();

            //_context.ImageParserOutputs.Add(output);
            //await _context.SaveChangesAsync();
            var responseOutput = new ParseImageResponseDto
            {
                InputId = request.ChatId,
                ParsedData = aiResponse
            };
            return Ok(responseOutput);
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetImageParserChats()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var chats = await _context.ImageParserInputs
                .Where(i => i.Chat.UserId == userId)
                .Select(i => new
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
        public async Task<IActionResult> GetImageParserHistory(Guid chatId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Verify chat ownership
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId && c.UserId == userId);
            if (chat == null) return NotFound("Chat not found or you do not have access.");

            var history = await _context.ImageParserInputs
                .Where(i => i.ChatId == chatId)
                .Include(i => i.Output)
                .OrderBy(i => i.UploadedAt)
                .Select(i => new
                {
                    InputId = i.Id,
                    ImageUrl = i.ImageUrl,
                    JsonSchema = i.JsonSchema,
                    UploadedAt = i.UploadedAt,
                    ParsedData = i.Output != null ? i.Output.ParsedData : null,
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

            var currentUsage = await _context.ImageParserInputs
                .CountAsync(i => i.Chat.UserId == userId &&
                                 i.UploadedAt.Year == DateTime.UtcNow.Year &&
                                 i.UploadedAt.Month == DateTime.UtcNow.Month);

            return currentUsage < user.Subscription.MaxUsagePerMonth;
        }
    }


}
