using AvitoTestTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ReviewService.Controllers;

[ApiController]
public class PullRequestController : ControllerBase
{
    private readonly AppDbContext _context;

    public PullRequestController(AppDbContext context)
    {
        _context = context;
    }

    // Создание PR
    [HttpPost("pullRequest/create")]
    public async Task<IActionResult> CreatePr([FromBody] CreatePrRequest request)
    {
        // Если PR есть
        if (await _context.PullRequests.AnyAsync(p => p.Id == request.PullRequestId))
        {
            return Conflict(new ErrorResponse { Error = new ErrorDetail { Code = "PR_EXISTS", Message = "PR already exists" } });
        }

        // Поиск автора
        var author = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.AuthorId);
        if (author == null)
        {
            return NotFound(new ErrorResponse { Error = new ErrorDetail { Code = "NOT_FOUND", Message = "Author not found" } });
        }

        // Поиск (команда, активные, не автор)
        var candidates = await _context.Users
            .Where(u => u.TeamName == author.TeamName
                        && u.IsActive
                        && u.Id != author.Id)
            .ToListAsync();

        //  Выбор 2 случайных кандидатов
        var reviewers = candidates
            .OrderBy(x => Guid.NewGuid())
            .Take(2)
            .ToList();

        // Создание PR
        var pr = new PullRequest
        {
            Id = request.PullRequestId,
            Name = request.PullRequestName,
            AuthorId = request.AuthorId,
            Status = "OPEN",
            CreatedAt = DateTime.UtcNow,
            Reviewers = reviewers
        };

        _context.PullRequests.Add(pr);
        await _context.SaveChangesAsync();

        return StatusCode(201, new { pr = MapToResponse(pr) });
    }

    // Merge
    [HttpPost("pullRequest/merge")]
    public async Task<IActionResult> MergePr([FromBody] MergeRequest request)
    {
        var pr = await _context.PullRequests
            .Include(p => p.Reviewers)
            .FirstOrDefaultAsync(p => p.Id == request.PullRequestId);

        if (pr == null) return NotFound(new ErrorResponse { Error = new ErrorDetail { Code = "NOT_FOUND", Message = "PR not found" } });

        // Если уже MERGED, то просто возвращаем OK
        if (pr.Status == "MERGED") return Ok(new { pr = MapToResponse(pr) });

        pr.Status = "MERGED";
        pr.MergedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { pr = MapToResponse(pr) });
    }

    // Переназначение
    [HttpPost("pullRequest/reassign")]
    public async Task<IActionResult> Reassign([FromBody] ReassignRequest request)
    {
        var pr = await _context.PullRequests
            .Include(p => p.Reviewers)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == request.PullRequestId);

        if (pr == null) return NotFound(new ErrorResponse { Error = new ErrorDetail { Code = "NOT_FOUND", Message = "PR not found" } });

        if (pr.Status == "MERGED")
            return Conflict(new ErrorResponse { Error = new ErrorDetail { Code = "PR_MERGED", Message = "Cannot reassign on merged PR" } });

        var oldReviewer = pr.Reviewers.FirstOrDefault(u => u.Id == request.OldUserId);
        if (oldReviewer == null)
            return Conflict(new ErrorResponse { Error = new ErrorDetail { Code = "NOT_ASSIGNED", Message = "Reviewer is not assigned" } });

        // Поиск замены (команда, активен, не автор, не назначен на этот PR)
        var currentReviewerIds = pr.Reviewers.Select(r => r.Id).ToList();

        var candidates = await _context.Users
            .Where(u => u.TeamName == pr.Author.TeamName
                        && u.IsActive
                        && u.Id != pr.AuthorId
                        && !currentReviewerIds.Contains(u.Id))
            .ToListAsync();

        if (!candidates.Any())
            return Conflict(new ErrorResponse { Error = new ErrorDetail { Code = "NO_CANDIDATE", Message = "No candidates available" } });

        var newReviewer = candidates.OrderBy(x => Guid.NewGuid()).First();

        pr.Reviewers.Remove(oldReviewer);
        pr.Reviewers.Add(newReviewer);
        await _context.SaveChangesAsync();

        return Ok(new { pr = MapToResponse(pr), replaced_by = newReviewer.Id });
    }

    // Вывод в JSON
    private PullRequestResponse MapToResponse(PullRequest pr)
    {
        return new PullRequestResponse
        {
            Id = pr.Id,
            Name = pr.Name,
            AuthorId = pr.AuthorId,
            Status = pr.Status,
            AssignedReviewers = pr.Reviewers.Select(r => r.Id).ToList(),
            CreatedAt = pr.CreatedAt,
            MergedAt = pr.MergedAt
        };
    }
}