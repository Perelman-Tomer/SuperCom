using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SuperCom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedTaskItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "CreatedAt", "Description", "DueDate", "Priority", "Title", "UpdatedAt", "UserEmail", "UserFullName", "UserTelephone" },
                values: new object[,]
                {
                    { 100, new DateTime(2025, 12, 20, 8, 0, 0, 0, DateTimeKind.Utc), "Users can submit the login form with an empty password field. Add client-side and server-side validation.", new DateTime(2026, 1, 5, 10, 0, 0, 0, DateTimeKind.Utc), 2, "Fix login page validation bug", null, "dana.cohen@example.com", "Dana Cohen", "050-1234567" },
                    { 101, new DateTime(2025, 12, 28, 9, 30, 0, 0, DateTimeKind.Utc), "Cover all CRUD operations and edge cases. Target 90% code coverage for the service layer.", new DateTime(2026, 1, 12, 14, 0, 0, 0, DateTimeKind.Utc), 1, "Write unit tests for TaskService", null, "yossi.levi@example.com", "Yossi Levi", "052-9876543" },
                    { 102, new DateTime(2026, 1, 2, 11, 0, 0, 0, DateTimeKind.Utc), "Swagger docs are outdated after the latest endpoint changes. Update all request/response examples.", new DateTime(2026, 1, 18, 12, 0, 0, 0, DateTimeKind.Utc), 0, "Update API documentation", null, "noa.shapira@example.com", "Noa Shapira", "054-5551234" },
                    { 103, new DateTime(2026, 1, 8, 7, 45, 0, 0, DateTimeKind.Utc), "The tasks list endpoint takes over 3 seconds for large datasets. Profile and optimize the SQL queries.", new DateTime(2026, 1, 22, 9, 0, 0, 0, DateTimeKind.Utc), 3, "Investigate slow database queries", null, "amit.peretz@example.com", "Amit Peretz", "053-7778899" },
                    { 104, new DateTime(2026, 1, 10, 13, 0, 0, 0, DateTimeKind.Utc), "Configure GitHub Actions to build, test, and deploy on every push to main branch.", new DateTime(2026, 1, 28, 16, 0, 0, 0, DateTimeKind.Utc), 2, "Set up CI/CD pipeline", null, "shira.bd@example.com", "Shira Ben-David", "050-3334455" },
                    { 105, new DateTime(2026, 2, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Send email reminders 24 hours before a task's due date using the ReminderService.", new DateTime(2026, 3, 3, 9, 0, 0, 0, DateTimeKind.Utc), 2, "Implement task reminder notifications", null, "tomer.az@example.com", "Tomer Azulay", "052-1112233" },
                    { 106, new DateTime(2026, 2, 18, 14, 30, 0, 0, DateTimeKind.Utc), "Implement a theme toggle in the UI. Persist the user's preference in localStorage.", new DateTime(2026, 3, 10, 12, 0, 0, 0, DateTimeKind.Utc), 0, "Add dark mode support to frontend", null, "maya.gold@example.com", "Maya Goldstein", "054-6667788" },
                    { 107, new DateTime(2026, 2, 20, 8, 0, 0, 0, DateTimeKind.Utc), "Build a dashboard showing task counts by priority, overdue tasks, and completion trends.", new DateTime(2026, 3, 15, 11, 0, 0, 0, DateTimeKind.Utc), 1, "Create user dashboard with task statistics", null, "eyal.m@example.com", "Eyal Mizrachi", "050-8889900" },
                    { 108, new DateTime(2026, 2, 19, 9, 15, 0, 0, DateTimeKind.Utc), "Evaluate and plan migration from SQL Server to PostgreSQL for cost reduction in production.", new DateTime(2026, 3, 22, 15, 0, 0, 0, DateTimeKind.Utc), 3, "Migrate database to PostgreSQL", null, "lior.katz@example.com", "Lior Katz", "053-2223344" },
                    { 109, new DateTime(2026, 2, 21, 16, 0, 0, 0, DateTimeKind.Utc), "Allow users to export filtered task lists as CSV files from the frontend.", new DateTime(2026, 3, 28, 10, 0, 0, 0, DateTimeKind.Utc), 1, "Add export tasks to CSV feature", null, "rotem.h@example.com", "Rotem Haim", "052-4445566" }
                });

            migrationBuilder.InsertData(
                table: "TaskTags",
                columns: new[] { "TagId", "TaskItemId" },
                values: new object[,]
                {
                    { 1, 100 },
                    { 2, 100 },
                    { 7, 101 },
                    { 5, 102 },
                    { 2, 103 },
                    { 6, 103 },
                    { 8, 104 },
                    { 3, 105 },
                    { 4, 105 },
                    { 3, 106 },
                    { 3, 107 },
                    { 4, 107 },
                    { 6, 108 },
                    { 8, 108 },
                    { 3, 109 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 1, 100 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 2, 100 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 7, 101 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 5, 102 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 2, 103 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 6, 103 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 8, 104 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 3, 105 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 4, 105 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 3, 106 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 3, 107 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 4, 107 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 6, 108 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 8, 108 });

            migrationBuilder.DeleteData(
                table: "TaskTags",
                keyColumns: new[] { "TagId", "TaskItemId" },
                keyValues: new object[] { 3, 109 });

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 109);
        }
    }
}
