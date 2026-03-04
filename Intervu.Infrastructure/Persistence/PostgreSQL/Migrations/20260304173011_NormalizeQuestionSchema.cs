using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeQuestionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "Questions");

            migrationBuilder.AlterColumn<Guid>(
                name: "InterviewExperienceId",
                table: "Questions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHot",
                table: "Questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Round",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SaveCount",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Questions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Questions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Upvotes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Answers_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
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

            migrationBuilder.InsertData(
                table: "Answers",
                columns: new[] { "Id", "AuthorId", "Content", "CreatedAt", "IsVerified", "QuestionId", "UpdatedAt", "Upvotes" },
                values: new object[,]
                {
                    { new Guid("dd000001-0000-4000-8000-000000000001"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Use sliding window with a HashSet. O(n) time, O(min(m,n)) space where m is the charset size.", new DateTime(2026, 1, 10, 1, 0, 0, 0, DateTimeKind.Utc), true, new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new DateTime(2026, 1, 10, 1, 0, 0, 0, DateTimeKind.Utc), 42 },
                    { new Guid("dd000002-0000-4000-8000-000000000002"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Use consistent hashing, a KV store (Redis/DynamoDB), and a redirect service behind a CDN. Base62 encode the auto-increment ID.", new DateTime(2026, 1, 10, 2, 0, 0, 0, DateTimeKind.Utc), true, new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new DateTime(2026, 1, 10, 2, 0, 0, 0, DateTimeKind.Utc), 38 },
                    { new Guid("dd000003-0000-4000-8000-000000000003"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "== coerces types before comparison, === checks both type and value. Example: 0 == '' is true but 0 === '' is false.", new DateTime(2026, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), true, new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new DateTime(2026, 1, 15, 1, 0, 0, 0, DateTimeKind.Utc), 19 },
                    { new Guid("dd000004-0000-4000-8000-000000000004"), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), "Iterative: use prev, curr, next pointers. O(n) time, O(1) space. Recursive: return reversed rest, set head.next.next = head, head.next = null.", new DateTime(2026, 2, 1, 1, 0, 0, 0, DateTimeKind.Utc), true, new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new DateTime(2026, 2, 1, 1, 0, 0, 0, DateTimeKind.Utc), 31 }
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

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"),
                columns: new[] { "Category", "Content", "CreatedBy", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { 9, "Reverse a singly linked list. Provide both iterative and recursive solutions with time/space complexity analysis.", new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), 1, 7, 145, 2, "Reverse a Linked List", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 567 });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"),
                columns: new[] { "Category", "Content", "CreatedBy", "IsHot", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { 9, "Find the longest substring without repeating characters. Explain your approach and time complexity.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), true, 3, 2, 87, 2, "Longest Substring Without Repeating Characters", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), 342 });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"),
                columns: new[] { "Category", "Content", "CreatedBy", "IsHot", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { 3, "Design a URL shortener service like bit.ly. Discuss hashing strategy, data storage, redirect flow, analytics, and scaling.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), true, 3, 9, 234, 2, "Design a URL Shortener like bit.ly", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), 876 });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"),
                columns: new[] { "Category", "Content", "CreatedBy", "Level", "Round", "SaveCount", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { 2, "Explain the difference between == and === in JavaScript. Give examples where they produce different results.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 2, 2, 56, 2, "Explain == vs === in JavaScript", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 234 });

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
                table: "Answers",
                columns: new[] { "Id", "AuthorId", "Content", "CreatedAt", "IsVerified", "QuestionId", "UpdatedAt", "Upvotes" },
                values: new object[,]
                {
                    { new Guid("dd000005-0000-4000-8000-000000000005"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "Transformers use multi-head self-attention to capture long-range dependencies in parallel. Key components: Q/K/V matrices, positional encoding, layer norm, feed-forward layers.", new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("bb000001-0000-4000-8000-000000000001"), new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), 56 },
                    { new Guid("dd000006-0000-4000-8000-000000000006"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "RAG: when you need up-to-date knowledge without retraining, lower cost. Fine-tuning: when you need domain-specific behavior/style, smaller model. Hybrid approaches work best.", new DateTime(2026, 1, 23, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("bb000002-0000-4000-8000-000000000002"), new DateTime(2026, 1, 23, 0, 0, 0, 0, DateTimeKind.Utc), 43 },
                    { new Guid("dd000007-0000-4000-8000-000000000007"), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), "SELECT MAX(salary) FROM Employee WHERE salary < (SELECT MAX(salary) FROM Employee); Or: SELECT DISTINCT salary FROM Employee ORDER BY salary DESC LIMIT 1 OFFSET 1;", new DateTime(2026, 1, 12, 1, 0, 0, 0, DateTimeKind.Utc), true, new Guid("bb000004-0000-4000-8000-000000000004"), new DateTime(2026, 1, 12, 1, 0, 0, 0, DateTimeKind.Utc), 67 },
                    { new Guid("dd000008-0000-4000-8000-000000000008"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "Use RICE framework: Reach × Impact × Confidence / Effort. Combine with customer interviews, data analysis, and strategic alignment. Present trade-offs to stakeholders.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("bb000007-0000-4000-8000-000000000007"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 34 },
                    { new Guid("dd000009-0000-4000-8000-000000000009"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Token bucket for bursty traffic, sliding window log for precise limits. Use Redis with Lua scripts for distributed implementation. Consider API key vs IP-based limiting.", new DateTime(2026, 2, 9, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("bb000009-0000-4000-8000-000000000009"), new DateTime(2026, 2, 9, 0, 0, 0, 0, DateTimeKind.Utc), 45 }
                });

            migrationBuilder.InsertData(
                table: "Answers",
                columns: new[] { "Id", "AuthorId", "Content", "CreatedAt", "QuestionId", "UpdatedAt", "Upvotes" },
                values: new object[,]
                {
                    { new Guid("dd00000a-0000-4000-8000-00000000000a"), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), "Think of attention as a soft dictionary lookup. Query attends to all Keys to get weights, then combines Values. Multi-head allows different representation subspaces.", new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bb000001-0000-4000-8000-000000000001"), new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), 23 },
                    { new Guid("dd00000b-0000-4000-8000-00000000000b"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Using DENSE_RANK: WITH ranked AS (SELECT salary, DENSE_RANK() OVER (ORDER BY salary DESC) AS rnk FROM Employee) SELECT salary FROM ranked WHERE rnk = 2;", new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bb000004-0000-4000-8000-000000000004"), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), 29 }
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
                    { new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new Guid("aa000007-0000-4000-8000-000000000007") },
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
                    { new Guid("bb00000c-0000-4000-8000-00000000000c"), new Guid("aa000005-0000-4000-8000-000000000005") },
                    { new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new Guid("aa000007-0000-4000-8000-000000000007") },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new Guid("aa000003-0000-4000-8000-000000000003") },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new Guid("aa000005-0000-4000-8000-000000000005") },
                    { new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new Guid("aa000008-0000-4000-8000-000000000008") }
                });

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
                name: "IX_Answers_AuthorId",
                table: "Answers",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_Upvotes",
                table: "Answers",
                column: "Upvotes");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCompanies_CompanyId",
                table: "QuestionCompanies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionRoles_Role",
                table: "QuestionRoles",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTags_TagId",
                table: "QuestionTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_CreatedBy",
                table: "Questions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_CreatedBy",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "QuestionCompanies");

            migrationBuilder.DropTable(
                name: "QuestionRoles");

            migrationBuilder.DropTable(
                name: "QuestionTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Category",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CreatedAt",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CreatedBy",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_IsHot",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Level",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Status",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ViewCount",
                table: "Questions");

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000001-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000002-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000003-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000004-0000-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000005-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000006-0000-4000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000007-0000-4000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000008-0000-4000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb000009-0000-4000-8000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb00000a-0000-4000-8000-00000000000a"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb00000b-0000-4000-8000-00000000000b"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("bb00000c-0000-4000-8000-00000000000c"));

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "IsHot",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Round",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SaveCount",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Questions");

            migrationBuilder.AlterColumn<Guid>(
                name: "InterviewExperienceId",
                table: "Questions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "Questions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"),
                columns: new[] { "Answer", "Content", "QuestionType" },
                values: new object[] { "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.", "Reverse a linked list.", "Algorithm" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"),
                columns: new[] { "Answer", "Content", "QuestionType" },
                values: new object[] { "Use sliding window with a HashSet. O(n) time.", "Find the longest substring without repeating characters.", "Algorithm" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"),
                columns: new[] { "Answer", "Content", "QuestionType" },
                values: new object[] { "Use consistent hashing, a KV store, and a redirect service.", "Design a URL shortener like bit.ly.", "System Design" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"),
                columns: new[] { "Answer", "Content", "QuestionType" },
                values: new object[] { "== coerces types, === does not.", "Explain the difference between == and ===.", "JavaScript" });
        }
    }
}
