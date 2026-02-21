using Microsoft.EntityFrameworkCore;
using SuperCom.Core.Entities;
using SuperCom.Core.Enums;

namespace SuperCom.Infrastructure.Data;

public class SuperComDbContext : DbContext
{
    public SuperComDbContext(DbContextOptions<SuperComDbContext> options)
        : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TaskItem configuration
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(2000);
            entity.Property(t => t.UserFullName).HasMaxLength(150).IsRequired();
            entity.Property(t => t.UserTelephone).HasMaxLength(20).IsRequired();
            entity.Property(t => t.UserEmail).HasMaxLength(254).IsRequired();
            entity.Property(t => t.Priority).HasConversion<int>();
            entity.Property(t => t.IsCompleted).HasDefaultValue(false);
            entity.Property(t => t.ReminderSent).HasDefaultValue(false);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(t => t.RowVersion).IsRowVersion();
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(t => t.Name).IsUnique();
        });

        // TaskTag (junction table) configuration
        modelBuilder.Entity<TaskTag>(entity =>
        {
            entity.HasKey(tt => new { tt.TaskItemId, tt.TagId });

            entity.HasOne(tt => tt.TaskItem)
                  .WithMany(t => t.TaskTags)
                  .HasForeignKey(tt => tt.TaskItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tt => tt.Tag)
                  .WithMany(t => t.TaskTags)
                  .HasForeignKey(tt => tt.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed some default tags
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Urgent" },
            new Tag { Id = 2, Name = "Bug" },
            new Tag { Id = 3, Name = "Feature" },
            new Tag { Id = 4, Name = "Improvement" },
            new Tag { Id = 5, Name = "Documentation" },
            new Tag { Id = 6, Name = "Research" },
            new Tag { Id = 7, Name = "Testing" },
            new Tag { Id = 8, Name = "DevOps" }
        );

        // Seed 10 sample tasks (5 from last month - Jan 2026, 5 upcoming - Mar 2026)
        modelBuilder.Entity<TaskItem>().HasData(
            // ── Past tasks (January 2026 – overdue) ──
            new TaskItem
            {
                Id = 100, Title = "Fix login page validation bug",
                Description = "Users can submit the login form with an empty password field. Add client-side and server-side validation.",
                DueDate = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc),
                Priority = Priority.High,
                UserFullName = "Dana Cohen", UserTelephone = "050-1234567", UserEmail = "dana.cohen@example.com",
                CreatedAt = new DateTime(2025, 12, 20, 8, 0, 0, DateTimeKind.Utc),
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 101, Title = "Write unit tests for TaskService",
                Description = "Cover all CRUD operations and edge cases. Target 90% code coverage for the service layer.",
                DueDate = new DateTime(2026, 1, 12, 14, 0, 0, DateTimeKind.Utc),
                Priority = Priority.Medium,
                UserFullName = "Yossi Levi", UserTelephone = "052-9876543", UserEmail = "yossi.levi@example.com",
                CreatedAt = new DateTime(2025, 12, 28, 9, 30, 0, DateTimeKind.Utc),
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 102, Title = "Update API documentation",
                Description = "Swagger docs are outdated after the latest endpoint changes. Update all request/response examples.",
                DueDate = new DateTime(2026, 1, 18, 12, 0, 0, DateTimeKind.Utc),
                Priority = Priority.Low,
                UserFullName = "Noa Shapira", UserTelephone = "054-5551234", UserEmail = "noa.shapira@example.com",
                CreatedAt = new DateTime(2026, 1, 2, 11, 0, 0, DateTimeKind.Utc),
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 103, Title = "Investigate slow database queries",
                Description = "The tasks list endpoint takes over 3 seconds for large datasets. Profile and optimize the SQL queries.",
                DueDate = new DateTime(2026, 1, 22, 9, 0, 0, DateTimeKind.Utc),
                Priority = Priority.Critical,
                UserFullName = "Amit Peretz", UserTelephone = "053-7778899", UserEmail = "amit.peretz@example.com",
                CreatedAt = new DateTime(2026, 1, 8, 7, 45, 0, DateTimeKind.Utc),
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 104, Title = "Set up CI/CD pipeline",
                Description = "Configure GitHub Actions to build, test, and deploy on every push to main branch.",
                DueDate = new DateTime(2026, 1, 28, 16, 0, 0, DateTimeKind.Utc),
                Priority = Priority.High,
                UserFullName = "Shira Ben-David", UserTelephone = "050-3334455", UserEmail = "shira.bd@example.com",
                CreatedAt = new DateTime(2026, 1, 10, 13, 0, 0, DateTimeKind.Utc),
                IsCompleted = false
            },

            // ── Upcoming tasks (March 2026) ──
            new TaskItem
            {
                Id = 105, Title = "Implement task reminder notifications",
                Description = "Send email reminders 24 hours before a task's due date using the ReminderService.",
                DueDate = new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc),
                Priority = Priority.High,
                UserFullName = "Tomer Azulay", UserTelephone = "052-1112233", UserEmail = "tomer.az@example.com",
                CreatedAt = new DateTime(2026, 2, 15, 10, 0, 0, DateTimeKind.Utc),
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 106, Title = "Add dark mode support to frontend",
                Description = "Implement a theme toggle in the UI. Persist the user's preference in localStorage.",
                DueDate = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc),
                Priority = Priority.Low,
                UserFullName = "Maya Goldstein", UserTelephone = "054-6667788", UserEmail = "maya.gold@example.com",
                CreatedAt = new DateTime(2026, 2, 18, 14, 30, 0, DateTimeKind.Utc),
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 107, Title = "Create user dashboard with task statistics",
                Description = "Build a dashboard showing task counts by priority, overdue tasks, and completion trends.",
                DueDate = new DateTime(2026, 3, 15, 11, 0, 0, DateTimeKind.Utc),
                Priority = Priority.Medium,
                UserFullName = "Eyal Mizrachi", UserTelephone = "050-8889900", UserEmail = "eyal.m@example.com",
                CreatedAt = new DateTime(2026, 2, 20, 8, 0, 0, DateTimeKind.Utc),
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 108, Title = "Migrate database to PostgreSQL",
                Description = "Evaluate and plan migration from SQL Server to PostgreSQL for cost reduction in production.",
                DueDate = new DateTime(2026, 3, 22, 15, 0, 0, DateTimeKind.Utc),
                Priority = Priority.Critical,
                UserFullName = "Lior Katz", UserTelephone = "053-2223344", UserEmail = "lior.katz@example.com",
                CreatedAt = new DateTime(2026, 2, 19, 9, 15, 0, DateTimeKind.Utc),
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 109, Title = "Add export tasks to CSV feature",
                Description = "Allow users to export filtered task lists as CSV files from the frontend.",
                DueDate = new DateTime(2026, 3, 28, 10, 0, 0, DateTimeKind.Utc),
                Priority = Priority.Medium,
                UserFullName = "Rotem Haim", UserTelephone = "052-4445566", UserEmail = "rotem.h@example.com",
                CreatedAt = new DateTime(2026, 2, 21, 16, 0, 0, DateTimeKind.Utc),
                IsCompleted = false
            }
        );

        // Seed task-tag relationships for sample tasks
        modelBuilder.Entity<TaskTag>().HasData(
            // Task 100 (Fix login bug) → Bug, Urgent
            new TaskTag { TaskItemId = 100, TagId = 2 },
            new TaskTag { TaskItemId = 100, TagId = 1 },
            // Task 101 (Unit tests) → Testing
            new TaskTag { TaskItemId = 101, TagId = 7 },
            // Task 102 (API docs) → Documentation
            new TaskTag { TaskItemId = 102, TagId = 5 },
            // Task 103 (Slow queries) → Bug, Research
            new TaskTag { TaskItemId = 103, TagId = 2 },
            new TaskTag { TaskItemId = 103, TagId = 6 },
            // Task 104 (CI/CD) → DevOps
            new TaskTag { TaskItemId = 104, TagId = 8 },
            // Task 105 (Reminders) → Feature, Improvement
            new TaskTag { TaskItemId = 105, TagId = 3 },
            new TaskTag { TaskItemId = 105, TagId = 4 },
            // Task 106 (Dark mode) → Feature
            new TaskTag { TaskItemId = 106, TagId = 3 },
            // Task 107 (Dashboard) → Feature, Improvement
            new TaskTag { TaskItemId = 107, TagId = 3 },
            new TaskTag { TaskItemId = 107, TagId = 4 },
            // Task 108 (Migrate DB) → DevOps, Research
            new TaskTag { TaskItemId = 108, TagId = 8 },
            new TaskTag { TaskItemId = 108, TagId = 6 },
            // Task 109 (CSV export) → Feature
            new TaskTag { TaskItemId = 109, TagId = 3 }
        );
    }
}
