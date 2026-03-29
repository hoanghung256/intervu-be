using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddAvaluationStructureForInterviewType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvaluationStructure",
                table: "InterviewTypes",
                type: "jsonb",
                nullable: true);

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

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                columns: new[] { "EvaluationStructure", "MaxPrice", "MinPrice" },
                values: new object[] { "[\r\n    { \"Type\": \"Teamwork & Collaboration\", \"Question\": \"Based on the scenarios they shared, how effectively does the candidate collaborate with others, resolve conflicts, and support their teammates?\" },\r\n    { \"Type\": \"Adaptability & Working Under Pressure\", \"Question\": \"How does the candidate react to sudden changes in project requirements, tight deadlines, or high-pressure situations?\" },\r\n    { \"Type\": \"Ownership & Attitude\", \"Question\": \"Does the candidate demonstrate a strong sense of ownership (taking accountability for mistakes) and a proactive, growth-oriented mindset?\" },\r\n    { \"Type\": \"Professionalism Advice\", \"Question\": \"What specific advice would you give the candidate to improve their professionalism, interview etiquette, and overall impression on hiring managers?\" }\r\n]", 2000, 1000 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                columns: new[] { "EvaluationStructure", "MaxPrice", "MinPrice" },
                values: new object[] { "[\r\n    { \"Type\": \"Experience Authenticity\", \"Question\": \"How well does the candidate's explanation of their past work match the details on their CV? (e.g., Did they exaggerate their contributions? Do they deeply understand the projects they listed?)\" },\r\n    { \"Type\": \"Communication & Presentation\", \"Question\": \"How would you rate the candidate's communication skills, clarity of expression, and overall confidence during the interview?\" },\r\n    { \"Type\": \"Career Alignment\", \"Question\": \"Are the candidate's short-term and long-term career goals clear, realistic, and aligned with the typical progression in this field?\" },\r\n    { \"Type\": \"CV Improvement (Actionable Advice)\", \"Question\": \"What is the strongest highlight of their CV? Are there any red flags, formatting issues, or vague details they need to fix immediately?\" }\r\n]", 2000, 1000 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                columns: new[] { "EvaluationStructure", "MaxPrice", "MinPrice" },
                values: new object[] { "[\r\n    { \"Type\": \"Problem Solving & Logic\", \"Question\": \"How would you evaluate the candidate's ability to analyze requirements, clarify edge cases, and approach the problem logically before writing code?\" },\r\n    { \"Type\": \"Code Quality & Optimization\", \"Question\": \"Rate the candidate's code quality (clean code principles, naming conventions) and their ability to optimize for time and space complexity (Big O).\" },\r\n    { \"Type\": \"Tech Stack & Fundamentals\", \"Question\": \"Assess the candidate's grasp of core computer science fundamentals (OOP, Databases, System Design) and their proficiency in their primary tech stack/framework.\" },\r\n    { \"Type\": \"Actionable Tech Advice\", \"Question\": \"Where are the candidate's technical blind spots? Please list 1-3 specific technologies, concepts, or keywords they must study to improve.\" }\r\n]", 2000, 1000 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                columns: new[] { "EvaluationStructure", "MaxPrice", "MinPrice", "Status" },
                values: new object[] { "[\r\n    { \"Type\": \"Technical Readiness\", \"Question\": \"Summarize the candidate's technical competencies: Which areas meet the standard for their target level (Fresher/Junior/Mid/Senior), and which areas fall short?\" },\r\n    { \"Type\": \"Culture & Behavioral Fit\", \"Question\": \"Summarize their soft skills: Would this candidate be a solid cultural addition to a standard software engineering team?\" },\r\n    { \"Type\": \"Final Verdict\", \"Question\": \"If this were a real interview and you were the Hiring Manager, what would your decision be? (Strong Hire / Hire / Leaning Hire / No Hire) – Briefly explain your reasoning.\" },\r\n    { \"Type\": \"Top Priorities\", \"Question\": \"List the top 3 most critical action items the candidate must execute immediately to increase their chances of passing a real job interview.\" }\r\n]", 2000, 1000, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1466-f54f-7a12-a89e-3d459032ba89"));

            migrationBuilder.DeleteData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-7224-8808-39aa3e3b6377"));

            migrationBuilder.DeleteData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-74d5-8d8a-de2143f27c35"));

            migrationBuilder.DeleteData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cf"));

            migrationBuilder.DropColumn(
                name: "EvaluationStructure",
                table: "InterviewTypes");

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                columns: new[] { "MaxPrice", "MinPrice" },
                values: new object[] { 60, 15 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                columns: new[] { "MaxPrice", "MinPrice" },
                values: new object[] { 50, 10 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                columns: new[] { "MaxPrice", "MinPrice" },
                values: new object[] { 100, 30 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                columns: new[] { "MaxPrice", "MinPrice", "Status" },
                values: new object[] { 120, 40, 0 });
        }
    }
}
