
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
