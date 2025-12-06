using AI_ServiceProvider.Data;
using AI_ServiceProvider.DTOs;
using AI_ServiceProvider.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


namespace AI_ServiceProvider.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDTO request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var chat = new Chat { Name = request.Name, UserId = userId.Value };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return Ok(new ChatResponseDto { Id = chat.Id, Name = chat.Name, CreatedAt = chat.CreatedAt });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyChats()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var chats = await _context.Chats
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ChatResponseDto { Id = c.Id, Name = c.Name, CreatedAt = c.CreatedAt })
                .ToListAsync();

            return Ok(chats);
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

}
