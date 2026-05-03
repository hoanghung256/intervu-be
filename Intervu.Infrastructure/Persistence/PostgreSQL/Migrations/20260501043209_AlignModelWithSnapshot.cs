using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AlignModelWithSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                column: "EvaluationStructure",
                value: "[\n    { \"Type\": \"Teamwork & Collaboration\", \"Question\": \"Based on the scenarios they shared, how effectively does the candidate collaborate with others, resolve conflicts, and support their teammates?\" },\n    { \"Type\": \"Adaptability & Working Under Pressure\", \"Question\": \"How does the candidate react to sudden changes in project requirements, tight deadlines, or high-pressure situations?\" },\n    { \"Type\": \"Ownership & Attitude\", \"Question\": \"Does the candidate demonstrate a strong sense of ownership (taking accountability for mistakes) and a proactive, growth-oriented mindset?\" },\n    { \"Type\": \"Professionalism Advice\", \"Question\": \"What specific advice would you give the candidate to improve their professionalism, interview etiquette, and overall impression on hiring managers?\" }\n]");

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                column: "EvaluationStructure",
                value: "[\n    { \"Type\": \"Experience Authenticity\", \"Question\": \"How well does the candidate's explanation of their past work match the details on their CV? (e.g., Did they exaggerate their contributions? Do they deeply understand the projects they listed?)\" },\n    { \"Type\": \"Communication & Presentation\", \"Question\": \"How would you rate the candidate's communication skills, clarity of expression, and overall confidence during the interview?\" },\n    { \"Type\": \"Career Alignment\", \"Question\": \"Are the candidate's short-term and long-term career goals clear, realistic, and aligned with the typical progression in this field?\" },\n    { \"Type\": \"CV Improvement (Actionable Advice)\", \"Question\": \"What is the strongest highlight of their CV? Are there any red flags, formatting issues, or vague details they need to fix immediately?\" }\n]");

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                column: "EvaluationStructure",
                value: "[\n    { \"Type\": \"Problem Solving & Logic\", \"Question\": \"How would you evaluate the candidate's ability to analyze requirements, clarify edge cases, and approach the problem logically before writing code?\" },\n    { \"Type\": \"Code Quality & Optimization\", \"Question\": \"Rate the candidate's code quality (clean code principles, naming conventions) and their ability to optimize for time and space complexity (Big O).\" },\n    { \"Type\": \"Tech Stack & Fundamentals\", \"Question\": \"Assess the candidate's grasp of core computer science fundamentals (OOP, Databases, System Design) and their proficiency in their primary tech stack/framework.\" },\n    { \"Type\": \"Actionable Tech Advice\", \"Question\": \"Where are the candidate's technical blind spots? Please list 1-3 specific technologies, concepts, or keywords they must study to improve.\" }\n]");

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                column: "EvaluationStructure",
                value: "[\n    { \"Type\": \"Technical Readiness\", \"Question\": \"Summarize the candidate's technical competencies: Which areas meet the standard for their target level (Fresher/Junior/Mid/Senior), and which areas fall short?\" },\n    { \"Type\": \"Culture & Behavioral Fit\", \"Question\": \"Summarize their soft skills: Would this candidate be a solid cultural addition to a standard software engineering team?\" },\n    { \"Type\": \"Final Verdict\", \"Question\": \"If this were a real interview and you were the Hiring Manager, what would your decision be? (Strong Hire / Hire / Leaning Hire / No Hire) – Briefly explain your reasoning.\" },\n    { \"Type\": \"Top Priorities\", \"Question\": \"List the top 3 most critical action items the candidate must execute immediately to increase their chances of passing a real job interview.\" }\n]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                column: "EvaluationStructure",
                value: "[\r\n    { \"Type\": \"Teamwork & Collaboration\", \"Question\": \"Based on the scenarios they shared, how effectively does the candidate collaborate with others, resolve conflicts, and support their teammates?\" },\r\n    { \"Type\": \"Adaptability & Working Under Pressure\", \"Question\": \"How does the candidate react to sudden changes in project requirements, tight deadlines, or high-pressure situations?\" },\r\n    { \"Type\": \"Ownership & Attitude\", \"Question\": \"Does the candidate demonstrate a strong sense of ownership (taking accountability for mistakes) and a proactive, growth-oriented mindset?\" },\r\n    { \"Type\": \"Professionalism Advice\", \"Question\": \"What specific advice would you give the candidate to improve their professionalism, interview etiquette, and overall impression on hiring managers?\" }\r\n]");

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                column: "EvaluationStructure",
                value: "[\r\n    { \"Type\": \"Experience Authenticity\", \"Question\": \"How well does the candidate's explanation of their past work match the details on their CV? (e.g., Did they exaggerate their contributions? Do they deeply understand the projects they listed?)\" },\r\n    { \"Type\": \"Communication & Presentation\", \"Question\": \"How would you rate the candidate's communication skills, clarity of expression, and overall confidence during the interview?\" },\r\n    { \"Type\": \"Career Alignment\", \"Question\": \"Are the candidate's short-term and long-term career goals clear, realistic, and aligned with the typical progression in this field?\" },\r\n    { \"Type\": \"CV Improvement (Actionable Advice)\", \"Question\": \"What is the strongest highlight of their CV? Are there any red flags, formatting issues, or vague details they need to fix immediately?\" }\r\n]");

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                column: "EvaluationStructure",
                value: "[\r\n    { \"Type\": \"Problem Solving & Logic\", \"Question\": \"How would you evaluate the candidate's ability to analyze requirements, clarify edge cases, and approach the problem logically before writing code?\" },\r\n    { \"Type\": \"Code Quality & Optimization\", \"Question\": \"Rate the candidate's code quality (clean code principles, naming conventions) and their ability to optimize for time and space complexity (Big O).\" },\r\n    { \"Type\": \"Tech Stack & Fundamentals\", \"Question\": \"Assess the candidate's grasp of core computer science fundamentals (OOP, Databases, System Design) and their proficiency in their primary tech stack/framework.\" },\r\n    { \"Type\": \"Actionable Tech Advice\", \"Question\": \"Where are the candidate's technical blind spots? Please list 1-3 specific technologies, concepts, or keywords they must study to improve.\" }\r\n]");

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                column: "EvaluationStructure",
                value: "[\r\n    { \"Type\": \"Technical Readiness\", \"Question\": \"Summarize the candidate's technical competencies: Which areas meet the standard for their target level (Fresher/Junior/Mid/Senior), and which areas fall short?\" },\r\n    { \"Type\": \"Culture & Behavioral Fit\", \"Question\": \"Summarize their soft skills: Would this candidate be a solid cultural addition to a standard software engineering team?\" },\r\n    { \"Type\": \"Final Verdict\", \"Question\": \"If this were a real interview and you were the Hiring Manager, what would your decision be? (Strong Hire / Hire / Leaning Hire / No Hire) – Briefly explain your reasoning.\" },\r\n    { \"Type\": \"Top Priorities\", \"Question\": \"List the top 3 most critical action items the candidate must execute immediately to increase their chances of passing a real job interview.\" }\r\n]");
        }
    }
}
