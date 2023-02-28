using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboTaskEr.Data.Migrations
{
    /// <inheritdoc />
    public partial class Turbo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Tasks_TaaskId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TaaskId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "Titlu",
                table: "Tasks",
                newName: "Title");

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskId",
                table: "Comments",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Tasks_TaskId",
                table: "Comments",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Tasks_TaskId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TaskId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Tasks",
                newName: "Titlu");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TaaskId",
                table: "Comments",
                column: "TaaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Tasks_TaaskId",
                table: "Comments",
                column: "TaaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
