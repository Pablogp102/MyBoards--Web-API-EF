using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBoards.Migrations
{
    /// <inheritdoc />
    public partial class UserFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
             name: "FullName",
             table: "Users",
             type: "nvarchar(max)",
             nullable: true);

            migrationBuilder.Sql(
            @"UPDATE Users
                SET FullName = FirstName + ' ' + LastName
           ");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE Users
                    SET FirstName = (SELECT TOP 1 value FROM STRING_SPLIT(FullName, ' ')),
                    LastName = (SELECT TOP 1 value FROM STRING_SPLIT(FullName, ' ') ORDER BY (SELECT NULL OFFSET 1 ROW))
                ");
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users"
                );
        }
    }
}
