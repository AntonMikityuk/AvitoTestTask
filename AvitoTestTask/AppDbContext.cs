using AvitoTestTask.Models;
using Microsoft.EntityFrameworkCore;

namespace ReviewService;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Таблицы
    public DbSet<Team> Teams { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<PullRequest> PullRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Многие ко многим
        modelBuilder.Entity<PullRequest>()
            .HasMany(pr => pr.Reviewers)
            .WithMany(u => u.ReviewingPRs)
            .UsingEntity(j => j.ToTable("PrReviewers")); // Промежуточная таблица

        // Связь с автором
        modelBuilder.Entity<PullRequest>()
            .HasOne(pr => pr.Author)
            .WithMany()
            .HasForeignKey(pr => pr.AuthorId);
    }
}