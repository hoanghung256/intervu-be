using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants.PreparedQuestionConstants;
using System;

namespace Intervu.Domain.Entities
{
    /// <summary>
    /// A question the coach has prepared for a specific InterviewRoom ("session") prior
    /// to (and usable during) the live interview. Distinct from:
    ///   - Question:          the global question bank (authored globally).
    ///   - GeneratedQuestion: AI-extracted / post-interview approval flow.
    ///   - QuestionItem:      legacy JSONB stub on InterviewRoom.QuestionList.
    /// </summary>
    public class PreparedQuestion : EntityBase<Guid>
    {
        public Guid InterviewRoomId { get; set; }
        public virtual InterviewRoom InterviewRoom { get; set; } = null!;

        /// <summary>
        /// Coach (user) who created this prepared question.
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// When the question was imported from the question bank, this links back to
        /// the source Question.Id. Null for fully custom-authored questions.
        /// Snapshot semantics: edits to the bank row do NOT update this record.
        /// </summary>
        public Guid? SourceBankQuestionId { get; set; }

        /// <summary>
        /// Binary business switch:
        ///   NonCoding -> Mark as Asked (no candidate broadcast)
        ///   Coding    -> Send to Editor (broadcasts ReceiveProblem)
        /// </summary>
        public PreparedQuestionInteractionType InteractionType { get; set; }

        /// <summary>
        /// Free-form display label (e.g. "Behavioral", "Technical", "System Design",
        /// "Process", "Coding"). Sourced from the bank or inferred on custom create.
        /// Informational only; does NOT drive behavior.
        /// </summary>
        public string? DisplayCategoryLabel { get; set; }

        public string Title { get; set; } = string.Empty;

        /// <summary>Rich HTML (Quill output).</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Coding-only. e.g. "twoSum".</summary>
        public string? FunctionName { get; set; }

        /// <summary>
        /// Coding-only. Identical shape to InterviewRoom.TestCases
        /// (<c>[{ inputs: [{name, value}], expectedOutputs: [string] }]</c>) so we can
        /// copy it straight into the room fields on Send to Editor.
        /// JSON-serialized via EF fluent config.
        /// </summary>
        public object[]? TestCases { get; set; }

        public PreparedQuestionStatus Status { get; set; } = PreparedQuestionStatus.Pending;

        public DateTime? AskedAt { get; set; }

        /// <summary>Ordering within the prepared list for this room (ascending).</summary>
        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Legacy flag kept for API compatibility. Coding questions may be broadcast to the
        /// candidate editor without test cases or a finalized stub—the coach can finish setup in-room.
        /// </summary>
        public bool IsReadyForEditor() => true;
    }
}
