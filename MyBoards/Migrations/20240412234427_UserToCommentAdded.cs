using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBoards.Migrations
{
    /// <inheritdoc />
    public partial class UserToCommentAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commensts_WorkItems_WorkItemId",
                table: "Commensts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Commensts",
                table: "Commensts");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "Commensts");

            migrationBuilder.RenameTable(
                name: "Commensts",
                newName: "Comments");

            migrationBuilder.RenameIndex(
                name: "IX_Commensts_WorkItemId",
                table: "Comments",
                newName: "IX_Comments_WorkItemId");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comments",
                table: "Comments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_AuthorId",
                table: "Comments",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_WorkItems_WorkItemId",
                table: "Comments",
                column: "WorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_AuthorId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_WorkItems_WorkItemId",
                table: "Comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comments",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Comments");

            migrationBuilder.RenameTable(
                name: "Comments",
                newName: "Commensts");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_WorkItemId",
                table: "Commensts",
                newName: "IX_Commensts_WorkItemId");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Commensts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Commensts",
                table: "Commensts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Commensts_WorkItems_WorkItemId",
                table: "Commensts",
                column: "WorkItemId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
