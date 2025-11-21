using AvitoTestTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ReviewService.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // Установка активности
    [HttpPost("setIsActive")]
    public async Task<IActionResult> SetIsActive([FromBody] TeamMemberDto request)
    {
        // Поиск участника
        var user = await _context.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
            return NotFound(new ErrorResponse { Error = new ErrorDetail { Code = "NOT_FOUND", Message = "User not found" } });

        // Смена активности
        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            user = new
            {
                user_id = user.Id,
                username = user.Username,
                team_name = user.TeamName,
                is_active = user.IsActive
            }
        });
    }

    // Получение Ревью
    [HttpGet("getReview")]
    public async Task<IActionResult> GetReviews([FromQuery(Name = "user_id")] string userId)
    {
        // Поиск участника
        var user = await _context.Users
            .Include(u => u.ReviewingPRs)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new ErrorResponse { Error = new ErrorDetail { Code = "NOT_FOUND", Message = "User not found" } });

        // Если у участника нет PR - вывод пустого списка
        return Ok(new
        {
            user_id = userId,
            pull_requests = user.ReviewingPRs.Select(pr => new
            {
                pull_request_id = pr.Id,
                pull_request_name = pr.Name,
                author_id = pr.AuthorId,
                status = pr.Status
            }).ToList()
        });
    }
}