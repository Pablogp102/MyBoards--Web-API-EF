using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyBoards.Migrations
{
    /// <inheritdoc />
    public partial class TagDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Value" },
                values: new object[,]
                {
                    { 1, "Web" },
                    { 2, "UI" },
                    { 3, "Desktop" },
                    { 4, "API" },
                    { 5, "Service" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                 table: "Tags",
                 keyColumn: "Id",
                 keyValues: new object[] { 1, 2, 3, 4, 5 });
        }
    }
}
