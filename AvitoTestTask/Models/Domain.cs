using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AvitoTestTask.Models;

// DTO

// Ответ с ошибкой
public class ErrorResponse
{
    [JsonPropertyName("error")]
    public ErrorDetail Error { get; set; }
}

// Детали ошибки
public class ErrorDetail
{
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
}

// Команда
public class TeamDto
{
    [JsonPropertyName("team_name")]
    public string TeamName { get; set; }

    [JsonPropertyName("members")]
    public List<TeamMemberDto> Members { get; set; }
}

// Участник команды
public class TeamMemberDto
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}

// Ответ на PR
public class PullRequestResponse
{
    [JsonPropertyName("pull_request_id")]
    public string Id { get; set; }

    [JsonPropertyName("pull_request_name")]
    public string Name { get; set; }

    [JsonPropertyName("author_id")]
    public string AuthorId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("assigned_reviewers")]
    public List<string> AssignedReviewers { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("mergedAt")]
    public DateTime? MergedAt { get; set; }
}

// Запрос на переназначение
public class ReassignRequest
{
    [JsonPropertyName("pull_request_id")]
    public string PullRequestId { get; set; }

    [JsonPropertyName("old_user_id")]
    public string OldUserId { get; set; }
}

// Запрос на создание PR
public class CreatePrRequest
{
    [JsonPropertyName("pull_request_id")]
    public string PullRequestId { get; set; }
    [JsonPropertyName("pull_request_name")]
    public string PullRequestName { get; set; }
    [JsonPropertyName("author_id")]
    public string AuthorId { get; set; }
}
 // Запрос на слияние
public class MergeRequest
{
    [JsonPropertyName("pull_request_id")]
    public string PullRequestId { get; set; }
}

// Entities

// Команда
public class Team
{
    [Key]
    public string Name { get; set; }

    public List<User> Users { get; set; } = new();
}

// Участник команды
public class User
{
    [Key]
    public string Id { get; set; }

    public string Username { get; set; }
    public bool IsActive { get; set; }

    
    public string TeamName { get; set; }
    [ForeignKey("TeamName")]
    public Team Team { get; set; }

    // PR-ы, где уачстник назначен ревьером
    public List<PullRequest> ReviewingPRs { get; set; } = new();
}

// PR
public class PullRequest
{
    [Key]
    public string Id { get; set; }

    public string Name { get; set; }
    public string Status { get; set; } = "OPEN";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? MergedAt { get; set; }

    public string AuthorId { get; set; }
    [ForeignKey("AuthorId")]
    public User Author { get; set; }

    // Назначенные ревьюеры
    public List<User> Reviewers { get; set; } = new();
}