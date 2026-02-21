using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperCom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderSentToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 100,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 101,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 102,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 103,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 104,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 105,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 106,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 107,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 108,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 109,
                columns: new string[0],
                values: new object[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "Tasks");
        }
    }
}
