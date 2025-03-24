using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3H.Poll.Migration.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabase : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommonSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsSensitive = table.Column<bool>(type: "bit", nullable: false),
                    SettingType = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CommonSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sortby = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PaymentTerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    IsMultipleVotesAllowed = table.Column<bool>(type: "bit", nullable: false),
                    IsViewableByModerator = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    AccessCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeSettings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VotingFrequencyControl = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VotingCooldownMinutes = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Polls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    Auth0UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AzureAdB2CUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ColorAvatar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalViews = table.Column<int>(type: "int", nullable: true),
                    TotalVotes = table.Column<int>(type: "int", nullable: true),
                    AverageCompletionTime = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CompletionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DemographicsData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_PollAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollAnalytics_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    QuestionOrder = table.Column<int>(type: "int", nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PollId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_PasswordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvitationToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_PollInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollInvitations_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PollInvitations_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyPaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyPaymentTermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseTransactions_PaymentMethods_BuyPaymentMethodId",
                        column: x => x.BuyPaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseTransactions_PaymentTerms_BuyPaymentTermId",
                        column: x => x.BuyPaymentTermId,
                        principalTable: "PaymentTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseTransactions_Users_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VotingHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoterIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VotedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_VotingHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotingHistories_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VotingHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Choices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChoiceText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChoiceOrder = table.Column<int>(type: "int", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    MediaUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_Choices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Choices_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoteDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TextAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoterIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VoterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RealTimeCount = table.Column<bool>(type: "bit", nullable: false),
                    FilterOptions = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    VotedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_VoteDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoteDetails_Choices_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "Choices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_VoteDetails_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_VoteDetails_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Choices_QuestionId",
                table: "Choices",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_UserId",
                table: "PasswordHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnalytics_PollId",
                table: "PollAnalytics",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_PollInvitations_CreatedBy",
                table: "PollInvitations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PollInvitations_PollId",
                table: "PollInvitations",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_BuyPaymentMethodId",
                table: "PurchaseTransactions",
                column: "BuyPaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_BuyPaymentTermId",
                table: "PurchaseTransactions",
                column: "BuyPaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_SaleId",
                table: "PurchaseTransactions",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PollId",
                table: "Questions",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteDetails_ChoiceId",
                table: "VoteDetails",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteDetails_QuestionId",
                table: "VoteDetails",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteDetails_UserId",
                table: "VoteDetails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingHistories_PollId",
                table: "VotingHistories",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingHistories_UserId",
                table: "VotingHistories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommonSettings");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "PasswordHistory");

            migrationBuilder.DropTable(
                name: "PollAnalytics");

            migrationBuilder.DropTable(
                name: "PollInvitations");

            migrationBuilder.DropTable(
                name: "PurchaseTransactions");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "VoteDetails");

            migrationBuilder.DropTable(
                name: "VotingHistories");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "PaymentTerms");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Choices");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Polls");
        }
    }
}
