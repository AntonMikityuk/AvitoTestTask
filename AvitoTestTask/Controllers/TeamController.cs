using AvitoTestTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ReviewService.Controllers;

[ApiController]
[Route("team")]
public class TeamController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeamController(AppDbContext context)
    {
        _context = context;
    }

    // Добавление команды
    [HttpPost("add")]
    public async Task<IActionResult> AddTeam([FromBody] TeamDto request)
    {
        // Проверка на дубликаты
        if (await _context.Teams.AnyAsync(t => t.Name == request.TeamName))
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail { Code = "TEAM_EXISTS", Message = "team_name already exists" }
            });
        }

        // Создание команды и участников
        var team = new Team
        {
            Name = request.TeamName,
            Users = request.Members.Select(m => new User
            {
                Id = m.UserId,
                Username = m.Username,
                IsActive = m.IsActive,
                TeamName = request.TeamName
            }).ToList()
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return StatusCode(201, new { team = request });
    }

    // Получение команды
    [HttpGet("get")]
    public async Task<IActionResult> GetTeam([FromQuery(Name = "team_name")] string teamName)
    {
        var team = await _context.Teams
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Name == teamName);

        if (team == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = new ErrorDetail { Code = "NOT_FOUND", Message = "Team not found" }
            });
        }

        return Ok(new TeamDto
        {
            TeamName = team.Name,
            Members = team.Users.Select(u => new TeamMemberDto
            {
                UserId = u.Id,
                Username = u.Username,
                IsActive = u.IsActive
            }).ToList()
        });
    }
}