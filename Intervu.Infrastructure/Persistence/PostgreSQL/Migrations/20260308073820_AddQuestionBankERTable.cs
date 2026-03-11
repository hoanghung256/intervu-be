using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBankERTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LogoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsCoding = table.Column<bool>(type: "boolean", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    BasePrice = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    ProfilePicture = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SlugProfileUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CVUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PortfolioUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    CurrentAmount = table.Column<int>(type: "integer", nullable: false),
                    SavedQuestions = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateProfiles_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioUrl = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CurrentAmount = table.Column<int>(type: "integer", nullable: true),
                    ExperienceYears = table.Column<int>(type: "integer", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    BankBinNumber = table.Column<string>(type: "text", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SavedQuestions = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachProfiles_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewExperiences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: true),
                    LastRoundCompleted = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InterviewProcess = table.Column<string>(type: "text", nullable: false),
                    IsInterestedInContact = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewExperiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewExperiences_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewExperiences_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationReceives",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationReceives", x => new { x.NotificationId, x.ReceiverId });
                    table.ForeignKey(
                        name: "FK_NotificationReceives_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationReceives_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateSkills",
                columns: table => new
                {
                    CandidateProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSkills", x => new { x.CandidateProfilesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_CandidateSkills_CandidateProfiles_CandidateProfilesId",
                        column: x => x.CandidateProfilesId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Focus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReservingForUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachAvailabilities_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachAvailabilities_InterviewTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "InterviewTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                        column: x => x.ReservingForUserId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachCompanies",
                columns: table => new
                {
                    CoachProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompaniesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachCompanies", x => new { x.CoachProfilesId, x.CompaniesId });
                    table.ForeignKey(
                        name: "FK_CoachCompanies_CoachProfiles_CoachProfilesId",
                        column: x => x.CoachProfilesId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachCompanies_Companies_CompaniesId",
                        column: x => x.CompaniesId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachSkills",
                columns: table => new
                {
                    CoachProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachSkills", x => new { x.CoachProfilesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_CoachSkills_CoachProfiles_CoachProfilesId",
                        column: x => x.CoachProfilesId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    InterviewExperienceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    ViewCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Vote = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SaveCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsHot = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_InterviewExperiences_InterviewExperienceId",
                        column: x => x.InterviewExperienceId,
                        principalTable: "InterviewExperiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Questions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewBookingTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachAvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewBookingTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewBookingTransaction_CoachAvailabilities_CoachAvailabilityId",
                        column: x => x.CoachAvailabilityId,
                        principalTable: "CoachAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewBookingTransaction_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Vote = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsAnswer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionCompanies",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionCompanies", x => new { x.QuestionId, x.CompanyId });
                    table.ForeignKey(
                        name: "FK_QuestionCompanies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionCompanies_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionRoles",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionRoles", x => new { x.QuestionId, x.Role });
                    table.ForeignKey(
                        name: "FK_QuestionRoles_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTags",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTags", x => new { x.QuestionId, x.TagId });
                    table.ForeignKey(
                        name: "FK_QuestionTags_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuestionLikes",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuestionLikes", x => new { x.UserId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_UserQuestionLikes_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserQuestionLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentAvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    VideoCallRoomUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CurrentLanguage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LanguageCodes = table.Column<string>(type: "text", nullable: true),
                    ProblemDescription = table.Column<string>(type: "text", nullable: true),
                    ProblemShortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TestCases = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RescheduleAttemptCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_CandidateProfiles_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_CoachAvailabilities_CurrentAvailabilityId",
                        column: x => x.CurrentAvailabilityId,
                        principalTable: "CoachAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_InterviewBookingTransaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "InterviewBookingTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCommentLikes",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCommentLikes", x => new { x.UserId, x.CommentId });
                    table.ForeignKey(
                        name: "FK_UserCommentLikes_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCommentLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: false),
                    AIAnalysis = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_CandidateProfiles_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_InterviewRooms_InterviewRoomId",
                        column: x => x.InterviewRoomId,
                        principalTable: "InterviewRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterviewRescheduleRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentAvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedAvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RespondedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRescheduleRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_CoachAvailabilities_CurrentAvailabilityId",
                        column: x => x.CurrentAvailabilityId,
                        principalTable: "CoachAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_CoachAvailabilities_ProposedAvailabilityId",
                        column: x => x.ProposedAvailabilityId,
                        principalTable: "CoachAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_InterviewRooms_InterviewRoomId",
                        column: x => x.InterviewRoomId,
                        principalTable: "InterviewRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_Users_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_Users_RespondedBy",
                        column: x => x.RespondedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "LogoPath", "Name", "Website" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), "logos/google.png", "Google", "https://google.com" },
                    { new Guid("22222222-2222-4222-8222-222222222222"), "logos/meta.png", "Meta", "https://meta.com" },
                    { new Guid("33333333-3333-4333-8333-333333333333"), "logos/amazon.png", "Amazon", "https://amazon.com" },
                    { new Guid("44444444-4444-4444-8444-444444444444"), "logos/microsoft.png", "Microsoft", "https://microsoft.com" },
                    { new Guid("55555555-5555-4555-8555-555555555555"), "logos/netflix.png", "Netflix", "https://netflix.com" },
                    { new Guid("66666666-6666-4666-8666-666666666666"), "logos/tiktok.png", "TikTok", "https://tiktok.com" },
                    { new Guid("77777777-7777-4777-8777-777777777777"), "logos/apple.png", "Apple", "https://apple.com" },
                    { new Guid("88888888-8888-4888-8888-888888888888"), "logos/uber.png", "Uber", "https://uber.com" },
                    { new Guid("99999999-9999-4999-8999-999999999999"), "logos/spotify.png", "Spotify", "https://spotify.com" },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"), "logos/stripe.png", "Stripe", "https://stripe.com" },
                    { new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), "logos/shopee.png", "Shopee", "https://shopee.com" }
                });

            migrationBuilder.InsertData(
                table: "InterviewTypes",
                columns: new[] { "Id", "BasePrice", "Description", "DurationMinutes", "IsCoding", "Name", "Status" },
                values: new object[,]
                {
                    { new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"), 30, "Behavioral interview focused on communication and interpersonal skills.", 45, false, "Soft Skills Interview", 1 },
                    { new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"), 20, "Resume review and HR-style interview focusing on background and experience.", 30, false, "CV Interview", 1 },
                    { new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"), 50, "Technical interview with coding problems and system design questions.", 60, true, "Technical Interview", 1 },
                    { new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"), 70, "Full mock interview simulating a real job interview experience.", 75, true, "Mock Interview", 0 }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "CreatedAt", "Message", "Title" },
                values: new object[] { new Guid("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20"), new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Welcome to Intervu platform", "Welcome" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("01010101-0101-4101-8101-010101010101"), null, "Node.js" },
                    { new Guid("02020202-0202-4202-8202-020202020202"), null, "SQL" },
                    { new Guid("03030303-0303-4303-8303-030303030303"), null, "MongoDB" },
                    { new Guid("04040404-0404-4404-8404-040404040404"), null, "AWS" },
                    { new Guid("05050505-0505-4505-8505-050505050505"), null, "Azure" },
                    { new Guid("06060606-0606-4606-8606-060606060606"), null, "System Design" },
                    { new Guid("07070707-0707-4707-8707-070707070707"), null, "Microservices" },
                    { new Guid("08080808-0808-4808-8808-080808080808"), null, "Docker" },
                    { new Guid("09090909-0909-4909-8909-090909090909"), null, "Kubernetes" },
                    { new Guid("0a0a0a0a-0a0a-4a0a-8a0a-0a0a0a0a0a0a"), null, "Machine Learning" },
                    { new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1"), null, "C#" },
                    { new Guid("c2c2c2c2-c2c2-42c2-82c2-c2c2c2c2c2c2"), null, "Java" },
                    { new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3"), null, "JavaScript" },
                    { new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4"), null, "TypeScript" },
                    { new Guid("f5f5f5f5-f5f5-45f5-85f5-f5f5f5f5f5f5"), null, "React" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("aa000001-0000-4000-8000-000000000001"), "Artificial Intelligence & Machine Learning", "AI" },
                    { new Guid("aa000002-0000-4000-8000-000000000002"), "SQL & Database querying", "SQL" },
                    { new Guid("aa000003-0000-4000-8000-000000000003"), "Distributed systems & architecture design", "System Design" },
                    { new Guid("aa000004-0000-4000-8000-000000000004"), "Product management & strategy", "Product Strategy" },
                    { new Guid("aa000005-0000-4000-8000-000000000005"), "Backend engineering & APIs", "Backend" },
                    { new Guid("aa000006-0000-4000-8000-000000000006"), "Generative AI, LLMs, prompt engineering", "GenAI" },
                    { new Guid("aa000007-0000-4000-8000-000000000007"), "Data structures & algorithms", "Algorithms" },
                    { new Guid("aa000008-0000-4000-8000-000000000008"), "Frontend & UI engineering", "Frontend" },
                    { new Guid("aa000009-0000-4000-8000-000000000009"), "Behavioral & leadership questions", "Behavioral" },
                    { new Guid("aa00000a-0000-4000-8000-00000000000a"), "Data pipelines, ETL, big data", "Data Engineering" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Password", "ProfilePicture", "Role", "SlugProfileUrl", "Status" },
                values: new object[,]
                {
                    { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "alice@example.com", "Alice Candidate", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 0, "alice-candidate_1719000000001", 0 },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "bob@example.com", "Bob Coach", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, "bob-Coach_1719000000002", 0 },
                    { new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), "admin@example.com", "Admin", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 2, "admin_1719000000003", 0 },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), "john.doe@example.com", "John Doe", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, "john-doe_1719000000004", 0 },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), "sarah.lee@example.com", "Sarah Lee", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, "sarah-lee_1719000000005", 0 }
                });

            migrationBuilder.InsertData(
                table: "CandidateProfiles",
                columns: new[] { "Id", "Bio", "CVUrl", "CurrentAmount", "PortfolioUrl", "SavedQuestions" },
                values: new object[] { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Aspiring backend developer.", "https://example.com/cv-alice.pdf", 0, "https://portfolio.example.com/alice", null });

            migrationBuilder.InsertData(
                table: "CoachProfiles",
                columns: new[] { "Id", "BankAccountNumber", "BankBinNumber", "Bio", "CurrentAmount", "ExperienceYears", "PortfolioUrl", "SavedQuestions", "Status" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "", "", "Senior Backend Engineer with real interview experience", 0, 8, "https://portfolio.example.com/bob", null, 0 },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), "", "", "Fullstack Engineer previously at Uber", 0, 6, "https://portfolio.example.com/john", null, 0 },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), "", "", "Senior Frontend Engineer focusing on UI/UX interviews", 0, 7, "https://portfolio.example.com/sarah", null, 0 }
                });

            migrationBuilder.InsertData(
                table: "InterviewExperiences",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "CreatedBy", "InterviewProcess", "IsInterestedInContact", "LastRoundCompleted", "Level", "Role", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), new Guid("11111111-1111-4111-8111-111111111111"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Phone screen → 2 technical rounds → system design → behavioral", true, "Onsite", 3, "Software Engineer", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e"), new Guid("22222222-2222-4222-8222-222222222222"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Online assessment → coding interview → system design", false, "System Design", 2, "Frontend Developer", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f"), new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), "CV screening → HR call → technical interview with coding challenge", true, "Technical", 1, "Backend Engineer", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33") }
                });

            migrationBuilder.InsertData(
                table: "NotificationReceives",
                columns: new[] { "NotificationId", "ReceiverId" },
                values: new object[] { new Guid("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "IsHot", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { new Guid("bb000001-0000-4000-8000-000000000001"), 2, "Walk me through the Transformer architecture. How do self-attention mechanisms work and why are they superior to RNNs for sequence modeling?", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, true, 3, 2, 214, 2, "Explain the Transformer Architecture", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), 891 },
                    { new Guid("bb000002-0000-4000-8000-000000000002"), 2, "Compare Retrieval-Augmented Generation (RAG) with fine-tuning. In what scenarios would you choose one over the other for a production GenAI application?", new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, true, 3, 9, 178, 2, "RAG vs Fine-Tuning: When to Use Which?", new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), 654 }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("bb000003-0000-4000-8000-000000000003"), 3, "Design a customer support chatbot powered by a large language model. Address latency, hallucination mitigation, guardrails, and cost optimization.", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, 4, 9, 102, 2, "Design an LLM-Powered Customer Support Bot", new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Utc), 432 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "IsHot", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("bb000004-0000-4000-8000-000000000004"), 9, "Write a SQL query to find the second highest salary from an Employee table. Handle the case where there might be duplicate salaries.", new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), null, true, 1, 2, 456, 2, "Find the Second Highest Salary", new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), 1243 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { new Guid("bb000005-0000-4000-8000-000000000005"), 2, "You have a query that scans 50M rows and takes 30 seconds. Walk me through your approach to diagnose and optimize it.", new DateTime(2026, 1, 18, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, 3, 2, 134, 2, "Optimize a Slow Query with Millions of Rows", new DateTime(2026, 1, 18, 0, 0, 0, 0, DateTimeKind.Utc), 567 },
                    { new Guid("bb000006-0000-4000-8000-000000000006"), 9, "Explain SQL window functions. Write a query using ROW_NUMBER, RANK, and a running SUM to analyze monthly revenue data.", new DateTime(2026, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, 2, 7, 98, 2, "Window Functions: Running Total & Ranking", new DateTime(2026, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), 389 }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "IsHot", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("bb000007-0000-4000-8000-000000000007"), 7, "You're the PM for a new B2B SaaS product. You have 20 feature requests and resources for 5. Walk me through your prioritization framework.", new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, true, 3, 10, 189, 2, "How Would You Prioritize Features for a New Product?", new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), 723 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("bb000008-0000-4000-8000-000000000008"), 7, "You've just launched a new algorithmic feed for a social platform. What metrics would you track? How would you define success at 30/60/90 days?", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, 2, 10, 121, 2, "Define Success Metrics for a Social Media Feed", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), 456 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "IsHot", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("bb000009-0000-4000-8000-000000000009"), 3, "Design a distributed rate limiter for an API gateway. Discuss token bucket, sliding window, and their trade-offs at scale.", new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, true, 3, 9, 167, 2, "Design a Rate Limiter", new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), 678 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { new Guid("bb00000a-0000-4000-8000-00000000000a"), 8, "What are CQRS and Event Sourcing patterns? When would you adopt them and what are the operational challenges?", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, 4, 2, 89, 2, "Explain CQRS and Event Sourcing", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), 345 },
                    { new Guid("bb00000b-0000-4000-8000-00000000000b"), 8, "Compare microservices architecture with a monolith. What factors drive the decision and how do you handle the migration?", new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), null, 3, 10, 143, 2, "Microservices vs Monolith: Trade-offs", new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Utc), 512 },
                    { new Guid("bb00000c-0000-4000-8000-00000000000c"), 9, "Implement a thread-safe Singleton pattern in C#. Discuss Lazy<T>, double-checked locking, and when Singleton is an anti-pattern.", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, 2, 8, 67, 2, "Implement a Thread-Safe Singleton in C#", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 289 }
                });

            migrationBuilder.InsertData(
                table: "CandidateSkills",
                columns: new[] { "CandidateProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("02020202-0202-4202-8202-020202020202") },
                    { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") }
                });

            migrationBuilder.InsertData(
                table: "CoachAvailabilities",
                columns: new[] { "Id", "CoachId", "EndTime", "Focus", "ReservingForUserId", "StartTime", "Status", "TypeId" },
                values: new object[,]
                {
                    { new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 2, 10, 10, 0, 0, 0, DateTimeKind.Utc), 0, null, new DateTime(2026, 2, 10, 9, 0, 0, 0, DateTimeKind.Utc), 2, null },
                    { new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 3, 15, 15, 0, 0, 0, DateTimeKind.Utc), 0, null, new DateTime(2026, 3, 15, 14, 0, 0, 0, DateTimeKind.Utc), 0, null }
                });

            migrationBuilder.InsertData(
                table: "CoachCompanies",
                columns: new[] { "CoachProfilesId", "CompaniesId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("11111111-1111-4111-8111-111111111111") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("44444444-4444-4444-8444-444444444444") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("33333333-3333-4333-8333-333333333333") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("66666666-6666-4666-8666-666666666666") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("88888888-8888-4888-8888-888888888888") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("22222222-2222-4222-8222-222222222222") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("77777777-7777-4777-8777-777777777777") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("99999999-9999-4999-8999-999999999999") }
                });

            migrationBuilder.InsertData(
                table: "CoachSkills",
                columns: new[] { "CoachProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("02020202-0202-4202-8202-020202020202") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("06060606-0606-4606-8606-060606060606") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("07070707-0707-4707-8707-070707070707") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("08080808-0808-4808-8808-080808080808") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("04040404-0404-4404-8404-040404040404") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("07070707-0707-4707-8707-070707070707") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("09090909-0909-4909-8909-090909090909") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("0a0a0a0a-0a0a-4a0a-8a0a-0a0a0a0a0a0a") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("f5f5f5f5-f5f5-45f5-85f5-f5f5f5f5f5f5") }
                });

            migrationBuilder.InsertData(
                table: "QuestionCompanies",
                columns: new[] { "CompanyId", "QuestionId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("bb000001-0000-4000-8000-000000000001") },
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("bb000001-0000-4000-8000-000000000001") },
                    { new Guid("33333333-3333-4333-8333-333333333333"), new Guid("bb000001-0000-4000-8000-000000000001") },
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("bb000002-0000-4000-8000-000000000002") },
                    { new Guid("44444444-4444-4444-8444-444444444444"), new Guid("bb000002-0000-4000-8000-000000000002") },
                    { new Guid("33333333-3333-4333-8333-333333333333"), new Guid("bb000003-0000-4000-8000-000000000003") },
                    { new Guid("99999999-9999-4999-8999-999999999999"), new Guid("bb000003-0000-4000-8000-000000000003") },
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("bb000004-0000-4000-8000-000000000004") },
                    { new Guid("33333333-3333-4333-8333-333333333333"), new Guid("bb000004-0000-4000-8000-000000000004") },
                    { new Guid("44444444-4444-4444-8444-444444444444"), new Guid("bb000004-0000-4000-8000-000000000004") },
                    { new Guid("66666666-6666-4666-8666-666666666666"), new Guid("bb000005-0000-4000-8000-000000000005") },
                    { new Guid("99999999-9999-4999-8999-999999999999"), new Guid("bb000005-0000-4000-8000-000000000005") },
                    { new Guid("33333333-3333-4333-8333-333333333333"), new Guid("bb000006-0000-4000-8000-000000000006") },
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("bb000007-0000-4000-8000-000000000007") },
                    { new Guid("55555555-5555-4555-8555-555555555555"), new Guid("bb000007-0000-4000-8000-000000000007") },
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("bb000008-0000-4000-8000-000000000008") },
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("bb000009-0000-4000-8000-000000000009") },
                    { new Guid("99999999-9999-4999-8999-999999999999"), new Guid("bb000009-0000-4000-8000-000000000009") },
                    { new Guid("44444444-4444-4444-8444-444444444444"), new Guid("bb00000a-0000-4000-8000-00000000000a") },
                    { new Guid("33333333-3333-4333-8333-333333333333"), new Guid("bb00000b-0000-4000-8000-00000000000b") },
                    { new Guid("66666666-6666-4666-8666-666666666666"), new Guid("bb00000b-0000-4000-8000-00000000000b") },
                    { new Guid("44444444-4444-4444-8444-444444444444"), new Guid("bb00000c-0000-4000-8000-00000000000c") }
                });

            migrationBuilder.InsertData(
                table: "QuestionRoles",
                columns: new[] { "QuestionId", "Role" },
                values: new object[,]
                {
                    { new Guid("bb000001-0000-4000-8000-000000000001"), 4 },
                    { new Guid("bb000001-0000-4000-8000-000000000001"), 12 },
                    { new Guid("bb000002-0000-4000-8000-000000000002"), 2 },
                    { new Guid("bb000002-0000-4000-8000-000000000002"), 12 },
                    { new Guid("bb000003-0000-4000-8000-000000000003"), 12 },
                    { new Guid("bb000003-0000-4000-8000-000000000003"), 17 },
                    { new Guid("bb000004-0000-4000-8000-000000000004"), 2 },
                    { new Guid("bb000004-0000-4000-8000-000000000004"), 3 },
                    { new Guid("bb000005-0000-4000-8000-000000000005"), 3 },
                    { new Guid("bb000005-0000-4000-8000-000000000005"), 6 },
                    { new Guid("bb000006-0000-4000-8000-000000000006"), 3 },
                    { new Guid("bb000006-0000-4000-8000-000000000006"), 4 },
                    { new Guid("bb000007-0000-4000-8000-000000000007"), 1 },
                    { new Guid("bb000008-0000-4000-8000-000000000008"), 1 },
                    { new Guid("bb000009-0000-4000-8000-000000000009"), 2 },
                    { new Guid("bb000009-0000-4000-8000-000000000009"), 6 },
                    { new Guid("bb00000a-0000-4000-8000-00000000000a"), 6 },
                    { new Guid("bb00000a-0000-4000-8000-00000000000a"), 17 },
                    { new Guid("bb00000b-0000-4000-8000-00000000000b"), 2 },
                    { new Guid("bb00000b-0000-4000-8000-00000000000b"), 17 },
                    { new Guid("bb00000c-0000-4000-8000-00000000000c"), 2 },
                    { new Guid("bb00000c-0000-4000-8000-00000000000c"), 6 }
                });

            migrationBuilder.InsertData(
                table: "QuestionTags",
                columns: new[] { "QuestionId", "TagId" },
                values: new object[,]
                {
                    { new Guid("bb000001-0000-4000-8000-000000000001"), new Guid("aa000001-0000-4000-8000-000000000001") },
                    { new Guid("bb000001-0000-4000-8000-000000000001"), new Guid("aa000006-0000-4000-8000-000000000006") },
                    { new Guid("bb000002-0000-4000-8000-000000000002"), new Guid("aa000001-0000-4000-8000-000000000001") },
                    { new Guid("bb000002-0000-4000-8000-000000000002"), new Guid("aa000006-0000-4000-8000-000000000006") },
                    { new Guid("bb000003-0000-4000-8000-000000000003"), new Guid("aa000003-0000-4000-8000-000000000003") },
                    { new Guid("bb000003-0000-4000-8000-000000000003"), new Guid("aa000006-0000-4000-8000-000000000006") },
                    { new Guid("bb000004-0000-4000-8000-000000000004"), new Guid("aa000002-0000-4000-8000-000000000002") },
                    { new Guid("bb000004-0000-4000-8000-000000000004"), new Guid("aa00000a-0000-4000-8000-00000000000a") },
                    { new Guid("bb000005-0000-4000-8000-000000000005"), new Guid("aa000002-0000-4000-8000-000000000002") },
                    { new Guid("bb000005-0000-4000-8000-000000000005"), new Guid("aa000005-0000-4000-8000-000000000005") },
                    { new Guid("bb000006-0000-4000-8000-000000000006"), new Guid("aa000002-0000-4000-8000-000000000002") },
                    { new Guid("bb000006-0000-4000-8000-000000000006"), new Guid("aa00000a-0000-4000-8000-00000000000a") },
                    { new Guid("bb000007-0000-4000-8000-000000000007"), new Guid("aa000004-0000-4000-8000-000000000004") },
                    { new Guid("bb000007-0000-4000-8000-000000000007"), new Guid("aa000009-0000-4000-8000-000000000009") },
                    { new Guid("bb000008-0000-4000-8000-000000000008"), new Guid("aa000004-0000-4000-8000-000000000004") },
                    { new Guid("bb000009-0000-4000-8000-000000000009"), new Guid("aa000003-0000-4000-8000-000000000003") },
                    { new Guid("bb000009-0000-4000-8000-000000000009"), new Guid("aa000005-0000-4000-8000-000000000005") },
                    { new Guid("bb00000a-0000-4000-8000-00000000000a"), new Guid("aa000003-0000-4000-8000-000000000003") },
                    { new Guid("bb00000a-0000-4000-8000-00000000000a"), new Guid("aa000005-0000-4000-8000-000000000005") },
                    { new Guid("bb00000b-0000-4000-8000-00000000000b"), new Guid("aa000003-0000-4000-8000-000000000003") },
                    { new Guid("bb00000b-0000-4000-8000-00000000000b"), new Guid("aa000005-0000-4000-8000-000000000005") },
                    { new Guid("bb00000c-0000-4000-8000-00000000000c"), new Guid("aa000005-0000-4000-8000-000000000005") }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), 9, "Reverse a singly linked list. Provide both iterative and recursive solutions with time/space complexity analysis.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), new Guid("c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f"), 1, 7, 145, 2, "Reverse a Linked List", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 567 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "IsHot", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), 9, "Find the longest substring without repeating characters. Explain your approach and time complexity.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), true, 3, 2, 87, 2, "Longest Substring Without Repeating Characters", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), 342 },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), 3, "Design a URL shortener service like bit.ly. Discuss hashing strategy, data storage, redirect flow, analytics, and scaling.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), true, 3, 9, 234, 2, "Design a URL Shortener like bit.ly", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), 876 }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), 2, "Explain the difference between == and === in JavaScript. Give examples where they produce different results.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e"), 2, 2, 56, 2, "Explain == vs === in JavaScript", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 234 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c1c1c1c1-1111-4c11-8c11-c1c1c1c1c111"), "Use sliding window with a HashSet. O(n) time.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 10 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c2c2c2c2-2222-4c22-8c22-c2c2c2c2c222"), "You can also use a dictionary to map each character to its latest index for O(n) in a single pass.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 5 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c3c3c3c3-3333-4c33-8c33-c3c3c3c3c333"), "Use consistent hashing, a KV store, and a redirect service.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 8 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c4c4c4c4-4444-4c44-8c44-c4c4c4c4c444"), "Don't forget rate limiting and analytics counters when discussing the redirect service design.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 3 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c5c5c5c5-5555-4c55-8c55-c5c5c5c5c555"), "== coerces types, === does not.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 7 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c6c6c6c6-6666-4c66-8c66-c6c6c6c6c666"), "Always prefer === in modern JS/TS to avoid implicit coercion bugs. null == undefined is true but null === undefined is false.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 4 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c7c7c7c7-7777-4c77-8c77-c7c7c7c7c777"), "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.", new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), 9 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c8c8c8c8-8888-4c88-8c88-c8c8c8c8c888"), "Recursive solution is cleaner to write but costs O(n) stack space. Interviewers often ask you to do both.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 6 });

            migrationBuilder.InsertData(
                table: "InterviewBookingTransaction",
                columns: new[] { "Id", "Amount", "CoachAvailabilityId", "Status", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"), 1000, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"), 500, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), 1, 1, new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") }
                });

            migrationBuilder.InsertData(
                table: "QuestionCompanies",
                columns: new[] { "CompanyId", "QuestionId" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d") },
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a") },
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b") },
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b") },
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c") }
                });

            migrationBuilder.InsertData(
                table: "QuestionRoles",
                columns: new[] { "QuestionId", "Role" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), 2 },
                    { new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), 6 },
                    { new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), 2 },
                    { new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), 6 },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), 2 },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), 6 },
                    { new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), 7 },
                    { new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), 8 }
                });

            migrationBuilder.InsertData(
                table: "QuestionTags",
                columns: new[] { "QuestionId", "TagId" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new Guid("aa000007-0000-4000-8000-000000000007") },
                    { new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new Guid("aa000007-0000-4000-8000-000000000007") },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new Guid("aa000003-0000-4000-8000-000000000003") },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new Guid("aa000005-0000-4000-8000-000000000005") },
                    { new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new Guid("aa000008-0000-4000-8000-000000000008") }
                });

            migrationBuilder.InsertData(
                table: "InterviewRooms",
                columns: new[] { "Id", "CandidateId", "CoachId", "CurrentAvailabilityId", "CurrentLanguage", "DurationMinutes", "LanguageCodes", "ProblemDescription", "ProblemShortName", "RescheduleAttemptCount", "ScheduledTime", "Status", "TestCases", "TransactionId", "VideoCallRoomUrl" },
                values: new object[] { new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), null, 60, null, null, null, 0, new DateTime(2026, 2, 10, 9, 0, 0, 0, DateTimeKind.Utc), 0, null, new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"), "https://meet.example/room1" });

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "Id", "AIAnalysis", "CandidateId", "CoachId", "Comments", "InterviewRoomId", "Rating" },
                values: new object[] { new Guid("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10"), "{}", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "Great answers and communication.", new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"), 5 });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkills_SkillsId",
                table: "CandidateSkills",
                column: "SkillsId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_CoachId",
                table: "CoachAvailabilities",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_TypeId",
                table: "CoachAvailabilities",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachCompanies_CompaniesId",
                table: "CoachCompanies",
                column: "CompaniesId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachSkills_SkillsId",
                table: "CoachSkills",
                column: "SkillsId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_QuestionId",
                table: "Comments",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CandidateId",
                table: "Feedbacks",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CoachId",
                table: "Feedbacks",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_InterviewRoomId",
                table: "Feedbacks",
                column: "InterviewRoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_CoachAvailabilityId",
                table: "InterviewBookingTransaction",
                column: "CoachAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_UserId",
                table: "InterviewBookingTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewExperiences_CompanyId",
                table: "InterviewExperiences",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewExperiences_CreatedBy",
                table: "InterviewExperiences",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_CurrentAvailabilityId",
                table: "InterviewRescheduleRequests",
                column: "CurrentAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_ExpiresAt",
                table: "InterviewRescheduleRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_InterviewRoomId",
                table: "InterviewRescheduleRequests",
                column: "InterviewRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_InterviewRoomId_Status",
                table: "InterviewRescheduleRequests",
                columns: new[] { "InterviewRoomId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_ProposedAvailabilityId",
                table: "InterviewRescheduleRequests",
                column: "ProposedAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_RequestedBy",
                table: "InterviewRescheduleRequests",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_RespondedBy",
                table: "InterviewRescheduleRequests",
                column: "RespondedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_Status",
                table: "InterviewRescheduleRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CandidateId",
                table: "InterviewRooms",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CoachId",
                table: "InterviewRooms",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CurrentAvailabilityId",
                table: "InterviewRooms",
                column: "CurrentAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_TransactionId",
                table: "InterviewRooms",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationReceives_ReceiverId",
                table: "NotificationReceives",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId_ExpiresAt",
                table: "PasswordResetTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCompanies_CompanyId",
                table: "QuestionCompanies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionRoles_Role",
                table: "QuestionRoles",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Category",
                table: "Questions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreatedAt",
                table: "Questions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreatedBy",
                table: "Questions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_InterviewExperienceId",
                table: "Questions",
                column: "InterviewExperienceId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_IsHot",
                table: "Questions",
                column: "IsHot");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Level",
                table: "Questions",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Status",
                table: "Questions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ViewCount",
                table: "Questions",
                column: "ViewCount");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTags_TagId",
                table: "QuestionTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCommentLikes_CommentId",
                table: "UserCommentLikes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCommentLikes_UserId",
                table: "UserCommentLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionLikes_QuestionId",
                table: "UserQuestionLikes",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionLikes_UserId",
                table: "UserQuestionLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SlugProfileUrl",
                table: "Users",
                column: "SlugProfileUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateSkills");

            migrationBuilder.DropTable(
                name: "CoachCompanies");

            migrationBuilder.DropTable(
                name: "CoachSkills");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "InterviewRescheduleRequests");

            migrationBuilder.DropTable(
                name: "NotificationReceives");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "QuestionCompanies");

            migrationBuilder.DropTable(
                name: "QuestionRoles");

            migrationBuilder.DropTable(
                name: "QuestionTags");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UserCommentLikes");

            migrationBuilder.DropTable(
                name: "UserQuestionLikes");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "InterviewRooms");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "InterviewBookingTransaction");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "CoachAvailabilities");

            migrationBuilder.DropTable(
                name: "InterviewExperiences");

            migrationBuilder.DropTable(
                name: "CoachProfiles");

            migrationBuilder.DropTable(
                name: "InterviewTypes");

            migrationBuilder.DropTable(
                name: "CandidateProfiles");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
