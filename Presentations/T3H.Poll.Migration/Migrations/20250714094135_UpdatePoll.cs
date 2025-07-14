using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3H.Poll.Migration.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePoll : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PollSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ParticipantName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UserNameCreated = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserNameUpdated = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Filter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField1 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField2 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField3 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollSubmissions_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PollAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TextAnswer = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SingleChoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UserNameCreated = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserNameUpdated = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Filter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField1 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField2 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField3 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollAnswer_Choices_SingleChoiceId",
                        column: x => x.SingleChoiceId,
                        principalTable: "Choices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PollAnswer_PollSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "PollSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PollAnswer_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PollAnswerChoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RankOrder = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UserNameCreated = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserNameUpdated = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Filter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField1 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField2 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraField3 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollAnswerChoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollAnswerChoice_Choices_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "Choices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PollAnswerChoice_PollAnswer_PollAnswerId",
                        column: x => x.PollAnswerId,
                        principalTable: "PollAnswer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswer_AnsweredAt",
                table: "PollAnswer",
                column: "AnsweredAt");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswer_QuestionId",
                table: "PollAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswer_SingleChoiceId",
                table: "PollAnswer",
                column: "SingleChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswer_SubmissionId",
                table: "PollAnswer",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswerChoice_ChoiceId",
                table: "PollAnswerChoice",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswerChoice_PollAnswerId_ChoiceId",
                table: "PollAnswerChoice",
                columns: new[] { "PollAnswerId", "ChoiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PollSubmissions_ParticipantEmail",
                table: "PollSubmissions",
                column: "ParticipantEmail");

            migrationBuilder.CreateIndex(
                name: "IX_PollSubmissions_PollId",
                table: "PollSubmissions",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_PollSubmissions_SubmittedAt",
                table: "PollSubmissions",
                column: "SubmittedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollAnswerChoice");

            migrationBuilder.DropTable(
                name: "PollAnswer");

            migrationBuilder.DropTable(
                name: "PollSubmissions");
        }
    }
}
