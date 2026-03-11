using System;
using System.Collections.Generic;

namespace Intervu.Domain.Entities
{
    /// <summary>
    /// Lightweight snapshot of a Question stored as JSONB inside user profiles
    /// (SavedQuestions column). Avoids a DB join when rendering saved question cards.
    /// </summary>
    public class QuestionSnapshot
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Round { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int SaveCount { get; set; }
        public int Vote { get; set; }
        public bool IsHot { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorProfilePicture { get; set; }
        public string? AuthorSlug { get; set; }
        public List<string> CompanyNames { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }
}
