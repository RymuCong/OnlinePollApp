using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3H.Poll.Migration.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullMediaUrlChoice : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoteDetails_Choices_ChoiceId",
                table: "VoteDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteDetails_Questions_QuestionId",
                table: "VoteDetails");

            migrationBuilder.AlterColumn<string>(
                name: "MediaUrl",
                table: "Choices",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddForeignKey(
                name: "FK_VoteDetails_Choices_ChoiceId",
                table: "VoteDetails",
                column: "ChoiceId",
                principalTable: "Choices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VoteDetails_Questions_QuestionId",
                table: "VoteDetails",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoteDetails_Choices_ChoiceId",
                table: "VoteDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteDetails_Questions_QuestionId",
                table: "VoteDetails");

            migrationBuilder.AlterColumn<string>(
                name: "MediaUrl",
                table: "Choices",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VoteDetails_Choices_ChoiceId",
                table: "VoteDetails",
                column: "ChoiceId",
                principalTable: "Choices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoteDetails_Questions_QuestionId",
                table: "VoteDetails",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
