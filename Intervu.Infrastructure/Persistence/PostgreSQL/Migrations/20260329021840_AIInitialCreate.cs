using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AIInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AudioChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AudioData = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    RecordingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkSequenceNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioChunks", x => x.Id);
                });

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
                name: "Industries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Industries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsCoding = table.Column<bool>(type: "boolean", nullable: false),
                    MinPrice = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxPrice = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SuggestedDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    EvaluationStructure = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewTypes", x => x.Id);
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
                    CurrentJobTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ActionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
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
                name: "UserAssessmentAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Skill = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: true),
                    SelectedLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SfiaLevel = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssessmentAnswers", x => x.Id);
                    table.CheckConstraint("CK_UserAssessmentAnswers_SelectedLevel", "\"SelectedLevel\" IN ('None','Basic','Intermediate','Advanced')");
                    table.CheckConstraint("CK_UserAssessmentAnswers_SfiaLevel", "\"SfiaLevel\" IN (0,2,3,5)");
                    table.ForeignKey(
                        name: "FK_UserAssessmentAnswers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSkillAssessmentSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetJson = table.Column<string>(type: "jsonb", nullable: false),
                    CurrentJson = table.Column<string>(type: "jsonb", nullable: false),
                    GapJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkillAssessmentSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSkillAssessmentSnapshots_Users_UserId",
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
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "CoachIndustries",
                columns: table => new
                {
                    CoachProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndustriesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachIndustries", x => new { x.CoachProfilesId, x.IndustriesId });
                    table.ForeignKey(
                        name: "FK_CoachIndustries_CoachProfiles_CoachProfilesId",
                        column: x => x.CoachProfilesId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachIndustries_Industries_IndustriesId",
                        column: x => x.IndustriesId,
                        principalTable: "Industries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachInterviewServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachInterviewServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachInterviewServices_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachInterviewServices_InterviewTypes_InterviewTypeId",
                        column: x => x.InterviewTypeId,
                        principalTable: "InterviewTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "BookingRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CoachInterviewServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AimLevel = table.Column<int>(type: "integer", nullable: true),
                    JobDescriptionUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CVUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalAmount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingRequests_CandidateProfiles_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingRequests_CoachInterviewServices_CoachInterviewServiceId",
                        column: x => x.CoachInterviewServiceId,
                        principalTable: "CoachInterviewServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingRequests_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "QuestionReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionReports_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionReports_Users_ReportedBy",
                        column: x => x.ReportedBy,
                        principalTable: "Users",
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
                name: "InterviewBookingTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachAvailabilityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookedStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BookedDurationMinutes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewBookingTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewBookingTransaction_BookingRequests_BookingRequestId",
                        column: x => x.BookingRequestId,
                        principalTable: "BookingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "InterviewRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentAvailabilityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    VideoCallRoomUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CurrentLanguage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LanguageCodes = table.Column<string>(type: "text", nullable: true),
                    ProblemDescription = table.Column<string>(type: "text", nullable: true),
                    ProblemShortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TestCases = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RescheduleAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    BookingRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoachInterviewServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    AimLevel = table.Column<int>(type: "integer", nullable: true),
                    RoundNumber = table.Column<int>(type: "integer", nullable: true),
                    EvaluationStructure = table.Column<string>(type: "jsonb", nullable: true),
                    IsEvaluationCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_BookingRequests_BookingRequestId",
                        column: x => x.BookingRequestId,
                        principalTable: "BookingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_InterviewRooms_CoachInterviewServices_CoachInterviewServiceId",
                        column: x => x.CoachInterviewServiceId,
                        principalTable: "CoachInterviewServices",
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
                name: "GeneratedQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InterviewRoomId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedQuestions_InterviewRooms_InterviewRoomId",
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

            migrationBuilder.CreateTable(
                name: "InterviewRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachInterviewServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    InterviewRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewRounds_BookingRequests_BookingRequestId",
                        column: x => x.BookingRequestId,
                        principalTable: "BookingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewRounds_CoachInterviewServices_CoachInterviewServiceId",
                        column: x => x.CoachInterviewServiceId,
                        principalTable: "CoachInterviewServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRounds_InterviewRooms_InterviewRoomId",
                        column: x => x.InterviewRoomId,
                        principalTable: "InterviewRooms",
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
                table: "Industries",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("11110000-0000-4000-8000-000000000001"), "Fintech", "fintech" },
                    { new Guid("11110000-0000-4000-8000-000000000002"), "E-commerce", "e-commerce" },
                    { new Guid("11110000-0000-4000-8000-000000000003"), "EdTech", "edtech" },
                    { new Guid("11110000-0000-4000-8000-000000000004"), "Blockchain", "blockchain" },
                    { new Guid("11110000-0000-4000-8000-000000000005"), "HealthTech", "healthtech" },
                    { new Guid("11110000-0000-4000-8000-000000000006"), "SaaS", "saas" },
                    { new Guid("11110000-0000-4000-8000-000000000007"), "AI/ML", "ai-ml" },
                    { new Guid("11110000-0000-4000-8000-000000000008"), "GameDev", "gamedev" }
                });

            migrationBuilder.InsertData(
                table: "InterviewTypes",
                columns: new[] { "Id", "Description", "EvaluationStructure", "IsCoding", "MaxPrice", "MinPrice", "Name", "Status", "SuggestedDurationMinutes" },
                values: new object[,]
                {
                    { new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"), "Behavioral interview focused on communication and interpersonal skills.", "[\r\n    { \"Type\": \"Teamwork & Collaboration\", \"Question\": \"Based on the scenarios they shared, how effectively does the candidate collaborate with others, resolve conflicts, and support their teammates?\" },\r\n    { \"Type\": \"Adaptability & Working Under Pressure\", \"Question\": \"How does the candidate react to sudden changes in project requirements, tight deadlines, or high-pressure situations?\" },\r\n    { \"Type\": \"Ownership & Attitude\", \"Question\": \"Does the candidate demonstrate a strong sense of ownership (taking accountability for mistakes) and a proactive, growth-oriented mindset?\" },\r\n    { \"Type\": \"Professionalism Advice\", \"Question\": \"What specific advice would you give the candidate to improve their professionalism, interview etiquette, and overall impression on hiring managers?\" }\r\n]", false, 2000, 1000, "Soft Skills Interview", 1, 45 },
                    { new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"), "Resume review and HR-style interview focusing on background and experience.", "[\r\n    { \"Type\": \"Experience Authenticity\", \"Question\": \"How well does the candidate's explanation of their past work match the details on their CV? (e.g., Did they exaggerate their contributions? Do they deeply understand the projects they listed?)\" },\r\n    { \"Type\": \"Communication & Presentation\", \"Question\": \"How would you rate the candidate's communication skills, clarity of expression, and overall confidence during the interview?\" },\r\n    { \"Type\": \"Career Alignment\", \"Question\": \"Are the candidate's short-term and long-term career goals clear, realistic, and aligned with the typical progression in this field?\" },\r\n    { \"Type\": \"CV Improvement (Actionable Advice)\", \"Question\": \"What is the strongest highlight of their CV? Are there any red flags, formatting issues, or vague details they need to fix immediately?\" }\r\n]", false, 2000, 1000, "CV Interview", 1, 30 },
                    { new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"), "Technical interview with coding problems and system design questions.", "[\r\n    { \"Type\": \"Problem Solving & Logic\", \"Question\": \"How would you evaluate the candidate's ability to analyze requirements, clarify edge cases, and approach the problem logically before writing code?\" },\r\n    { \"Type\": \"Code Quality & Optimization\", \"Question\": \"Rate the candidate's code quality (clean code principles, naming conventions) and their ability to optimize for time and space complexity (Big O).\" },\r\n    { \"Type\": \"Tech Stack & Fundamentals\", \"Question\": \"Assess the candidate's grasp of core computer science fundamentals (OOP, Databases, System Design) and their proficiency in their primary tech stack/framework.\" },\r\n    { \"Type\": \"Actionable Tech Advice\", \"Question\": \"Where are the candidate's technical blind spots? Please list 1-3 specific technologies, concepts, or keywords they must study to improve.\" }\r\n]", true, 2000, 1000, "Technical Interview", 1, 60 },
                    { new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"), "Full mock interview simulating a real job interview experience.", "[\r\n    { \"Type\": \"Technical Readiness\", \"Question\": \"Summarize the candidate's technical competencies: Which areas meet the standard for their target level (Fresher/Junior/Mid/Senior), and which areas fall short?\" },\r\n    { \"Type\": \"Culture & Behavioral Fit\", \"Question\": \"Summarize their soft skills: Would this candidate be a solid cultural addition to a standard software engineering team?\" },\r\n    { \"Type\": \"Final Verdict\", \"Question\": \"If this were a real interview and you were the Hiring Manager, what would your decision be? (Strong Hire / Hire / Leaning Hire / No Hire) – Briefly explain your reasoning.\" },\r\n    { \"Type\": \"Top Priorities\", \"Question\": \"List the top 3 most critical action items the candidate must execute immediately to increase their chances of passing a real job interview.\" }\r\n]", true, 2000, 1000, "Mock Interview", 1, 75 }
                });

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
                columns: new[] { "Id", "BankAccountNumber", "BankBinNumber", "Bio", "CurrentAmount", "CurrentJobTitle", "ExperienceYears", "PortfolioUrl", "SavedQuestions", "Status" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "", "", "Senior Backend Engineer with real interview experience", 0, "Senior Backend Engineer", 8, "https://portfolio.example.com/bob", null, 0 },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), "", "", "Fullstack Engineer previously at Uber", 0, "Technical Lead", 6, "https://portfolio.example.com/john", null, 0 },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), "", "", "Senior Frontend Engineer focusing on UI/UX interviews", 0, "Senior Frontend Engineer", 7, "https://portfolio.example.com/sarah", null, 0 }
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
                table: "Notifications",
                columns: new[] { "Id", "ActionUrl", "CreatedAt", "Message", "ReferenceId", "Title", "Type", "UserId" },
                values: new object[] { new Guid("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20"), null, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Welcome to Intervu platform", null, "Welcome", 10, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

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
                columns: new[] { "Id", "CoachId", "EndTime", "StartTime", "Status" },
                values: new object[,]
                {
                    { new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 2, 10, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 10, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 3, 15, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 15, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 4, 1, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 1, 10, 0, 0, 0, DateTimeKind.Utc), 0 }
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
                table: "CoachIndustries",
                columns: new[] { "CoachProfilesId", "IndustriesId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("11110000-0000-4000-8000-000000000001") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("11110000-0000-4000-8000-000000000006") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("11110000-0000-4000-8000-000000000002") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("11110000-0000-4000-8000-000000000007") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("11110000-0000-4000-8000-000000000003") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("11110000-0000-4000-8000-000000000008") }
                });

            migrationBuilder.InsertData(
                table: "CoachInterviewServices",
                columns: new[] { "Id", "CoachId", "DurationMinutes", "InterviewTypeId", "Price" },
                values: new object[,]
                {
                    { new Guid("019d1466-f54f-7a12-a89e-3d459032ba89"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 30, new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"), 2000 },
                    { new Guid("019d1467-d415-7224-8808-39aa3e3b6377"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 45, new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"), 2000 },
                    { new Guid("019d1467-d415-74d5-8d8a-de2143f27c35"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 60, new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"), 2000 },
                    { new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cf"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 75, new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"), 2000 }
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
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "Level", "Round", "Status", "Title", "UpdatedAt" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), 9, "Reverse a singly linked list. Provide both iterative and recursive solutions with time/space complexity analysis.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), new Guid("c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f"), 1, 7, 2, "Reverse a Linked List", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "IsHot", "Level", "Round", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), 9, "Find the longest substring without repeating characters. Explain your approach and time complexity.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), true, 3, 2, 2, "Longest Substring Without Repeating Characters", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), 3, "Design a URL shortener service like bit.ly. Discuss hashing strategy, data storage, redirect flow, analytics, and scaling.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), true, 3, 9, 2, "Design a URL Shortener like bit.ly", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "Content", "CreatedAt", "CreatedBy", "InterviewExperienceId", "Level", "Round", "Status", "Title", "UpdatedAt" },
                values: new object[] { new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), 2, "Explain the difference between == and === in JavaScript. Give examples where they produce different results.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e"), 2, 2, 2, "Explain == vs === in JavaScript", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy" },
                values: new object[] { new Guid("c1c1c1c1-1111-4c11-8c11-c1c1c1c1c111"), "Use sliding window with a HashSet. O(n) time.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy" },
                values: new object[] { new Guid("c2c2c2c2-2222-4c22-8c22-c2c2c2c2c222"), "You can also use a dictionary to map each character to its latest index for O(n) in a single pass.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy" },
                values: new object[] { new Guid("c3c3c3c3-3333-4c33-8c33-c3c3c3c3c333"), "Use consistent hashing, a KV store, and a redirect service.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy" },
                values: new object[] { new Guid("c4c4c4c4-4444-4c44-8c44-c4c4c4c4c444"), "Don't forget rate limiting and analytics counters when discussing the redirect service design.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy" },
                values: new object[] { new Guid("c5c5c5c5-5555-4c55-8c55-c5c5c5c5c555"), "== coerces types, === does not.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy" },
                values: new object[] { new Guid("c6c6c6c6-6666-4c66-8c66-c6c6c6c6c666"), "Always prefer === in modern JS/TS to avoid implicit coercion bugs. null == undefined is true but null === undefined is false.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy" },
                values: new object[] { new Guid("c7c7c7c7-7777-4c77-8c77-c7c7c7c7c777"), "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.", new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33") });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy" },
                values: new object[] { new Guid("c8c8c8c8-8888-4c88-8c88-c8c8c8c8c888"), "Recursive solution is cleaner to write but costs O(n) stack space. Interviewers often ask you to do both.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.InsertData(
                table: "InterviewBookingTransaction",
                columns: new[] { "Id", "Amount", "BookedDurationMinutes", "BookedStartTime", "BookingRequestId", "CoachAvailabilityId", "CoachId", "Status", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00"), 2000, null, null, null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"), null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"), 1000, null, null, null, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99"), 1500, null, null, null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"), null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"), 500, null, null, null, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), null, 1, 1, new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") }
                });

            migrationBuilder.InsertData(
                table: "QuestionCompanies",
                columns: new[] { "CompanyId", "QuestionId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-8444-444444444444"), new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d") },
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a") },
                    { new Guid("66666666-6666-4666-8666-666666666666"), new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b") },
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
                columns: new[] { "Id", "AimLevel", "BookingRequestId", "CandidateId", "CoachId", "CoachInterviewServiceId", "CurrentAvailabilityId", "CurrentLanguage", "DurationMinutes", "EvaluationStructure", "IsEvaluationCompleted", "LanguageCodes", "ProblemDescription", "ProblemShortName", "RescheduleAttemptCount", "RoundNumber", "ScheduledTime", "Status", "TestCases", "TransactionId", "Type", "VideoCallRoomUrl" },
                values: new object[,]
                {
                    { new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), null, 60, null, false, null, null, null, 0, null, new DateTime(2026, 2, 10, 9, 0, 0, 0, DateTimeKind.Utc), 0, null, new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"), 0, "https://meet.example/room1" },
                    { new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a77"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"), null, 60, null, false, null, null, null, 0, null, new DateTime(2026, 3, 15, 14, 30, 0, 0, DateTimeKind.Utc), 1, null, new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99"), 1, "https://meet.example/room-ai" },
                    { new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a88"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"), null, 60, null, false, null, null, null, 0, null, new DateTime(2026, 4, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1, null, new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00"), 0, "https://meet.example/room3" }
                });

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "Id", "AIAnalysis", "CandidateId", "CoachId", "Comments", "InterviewRoomId", "Rating" },
                values: new object[] { new Guid("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10"), "{}", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "Great answers and communication.", new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"), 5 });

            migrationBuilder.CreateIndex(
                name: "IX_AudioChunks_RecordingSession_Sequence",
                table: "AudioChunks",
                columns: new[] { "RecordingSessionId", "ChunkSequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AudioChunks_RecordingSessionId",
                table: "AudioChunks",
                column: "RecordingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_CandidateId",
                table: "BookingRequests",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_CoachId",
                table: "BookingRequests",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_CoachInterviewServiceId",
                table: "BookingRequests",
                column: "CoachInterviewServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_ExpiresAt",
                table: "BookingRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_Status",
                table: "BookingRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkills_SkillsId",
                table: "CandidateSkills",
                column: "SkillsId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_CoachId",
                table: "CoachAvailabilities",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachCompanies_CompaniesId",
                table: "CoachCompanies",
                column: "CompaniesId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachIndustries_IndustriesId",
                table: "CoachIndustries",
                column: "IndustriesId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachInterviewServices_CoachId_InterviewTypeId",
                table: "CoachInterviewServices",
                columns: new[] { "CoachId", "InterviewTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachInterviewServices_InterviewTypeId",
                table: "CoachInterviewServices",
                column: "InterviewTypeId");

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
                name: "IX_GeneratedQuestions_InterviewRoomId",
                table: "GeneratedQuestions",
                column: "InterviewRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Industries_Name",
                table: "Industries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Industries_Slug",
                table: "Industries",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_BookingRequestId",
                table: "InterviewBookingTransaction",
                column: "BookingRequestId");

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
                name: "IX_InterviewRooms_BookingRequestId",
                table: "InterviewRooms",
                column: "BookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CandidateId",
                table: "InterviewRooms",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CoachId",
                table: "InterviewRooms",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CoachInterviewServiceId",
                table: "InterviewRooms",
                column: "CoachInterviewServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CurrentAvailabilityId",
                table: "InterviewRooms",
                column: "CurrentAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_TransactionId",
                table: "InterviewRooms",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRounds_BookingRequestId_RoundNumber",
                table: "InterviewRounds",
                columns: new[] { "BookingRequestId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRounds_CoachInterviewServiceId",
                table: "InterviewRounds",
                column: "CoachInterviewServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRounds_InterviewRoomId",
                table: "InterviewRounds",
                column: "InterviewRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

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
                name: "IX_QuestionReports_QuestionId",
                table: "QuestionReports",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionReports_ReportedBy",
                table: "QuestionReports",
                column: "ReportedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionReports_Status",
                table: "QuestionReports",
                column: "Status");

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
                name: "IX_UserAssessmentAnswers_AssessmentId",
                table: "UserAssessmentAnswers",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAnswers_Skill",
                table: "UserAssessmentAnswers",
                column: "Skill");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAnswers_UserId",
                table: "UserAssessmentAnswers",
                column: "UserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_UserSkillAssessmentSnapshots_UserId",
                table: "UserSkillAssessmentSnapshots",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioChunks");

            migrationBuilder.DropTable(
                name: "CandidateSkills");

            migrationBuilder.DropTable(
                name: "CoachCompanies");

            migrationBuilder.DropTable(
                name: "CoachIndustries");

            migrationBuilder.DropTable(
                name: "CoachSkills");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "GeneratedQuestions");

            migrationBuilder.DropTable(
                name: "InterviewRescheduleRequests");

            migrationBuilder.DropTable(
                name: "InterviewRounds");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "QuestionCompanies");

            migrationBuilder.DropTable(
                name: "QuestionReports");

            migrationBuilder.DropTable(
                name: "QuestionRoles");

            migrationBuilder.DropTable(
                name: "QuestionTags");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UserAssessmentAnswers");

            migrationBuilder.DropTable(
                name: "UserCommentLikes");

            migrationBuilder.DropTable(
                name: "UserQuestionLikes");

            migrationBuilder.DropTable(
                name: "UserSkillAssessmentSnapshots");

            migrationBuilder.DropTable(
                name: "Industries");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "InterviewRooms");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "InterviewBookingTransaction");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "BookingRequests");

            migrationBuilder.DropTable(
                name: "CoachAvailabilities");

            migrationBuilder.DropTable(
                name: "InterviewExperiences");

            migrationBuilder.DropTable(
                name: "CandidateProfiles");

            migrationBuilder.DropTable(
                name: "CoachInterviewServices");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "CoachProfiles");

            migrationBuilder.DropTable(
                name: "InterviewTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
