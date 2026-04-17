using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Intervu.Domain.Entities
{
    public class InterviewRoom : EntityBase<Guid>
    {
        /// <summary>
        /// EntityBase.Id represents InterviewRoomId
        /// </summary>
        public Guid? CandidateId { get; set; }

        public Guid? CoachId { get; set; }

        public Guid? TransactionId { get; set; }

        /// <summary>
        /// Flow A: links to the availability range. Nullable for Flow B/C.
        /// </summary>
        public Guid? CurrentAvailabilityId { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public int? DurationMinutes { get; set; }

        public string? VideoCallRoomUrl { get; set; }

        public string? CurrentLanguage { get; set; }

        public Dictionary<string, string>? LanguageCodes { get; set; }

        public string? ProblemDescription { get; set; }

        public string? ProblemShortName { get; set; }

        public object[]? TestCases { get; set; }

        /// <summary>
        /// Scheduled, Completed, Cancelled, No-Show
        /// </summary>
        public InterviewRoomStatus Status { get; set; }

        public int RescheduleAttemptCount { get; set; } = 0;

        public InterviewRoomType Type { get; set; }

        // --- New booking context fields ---

        /// <summary>
        /// FK to BookingRequest (for Flow B/C rooms)
        /// </summary>
        public Guid? BookingRequestId { get; set; }

        /// <summary>
        /// FK to CoachInterviewService — the service type for this room
        /// </summary>
        public Guid? CoachInterviewServiceId { get; set; }

        /// <summary>
        /// Target interview level (Junior, MidLevel, Senior, TeamLeader)
        /// </summary>
        public AimLevel? AimLevel { get; set; }

        /// <summary>
        /// Which round number this room belongs to (for JD multi-round Flow C)
        /// </summary>
        public int? RoundNumber { get; set; }

        // Store the evaluation structure as a JSON string in the database, EF purpose
        [Column(TypeName = "jsonb")]
        public string? EvaluationResultsJson { get; set; }

        // Use this field for application logic; it will be ignored by EF Core and not mapped to the database
        [NotMapped]
        public List<EvaluationResult>? EvaluationResults
        {
            get
            {
                if (string.IsNullOrEmpty(EvaluationResultsJson))
                {
                    return null;
                }

                try
                {
                    // Legacy format: raw JSON array of evaluation results
                    var asArray = JsonSerializer.Deserialize<List<EvaluationResult>>(EvaluationResultsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (asArray != null)
                    {
                        return asArray;
                    }
                }
                catch
                {
                    // ignore and try object format below
                }

                try
                {
                    // New format: object containing results + metadata (others, hireDecision)
                    using var doc = JsonDocument.Parse(EvaluationResultsJson);
                    var root = doc.RootElement;
                    if (root.ValueKind != JsonValueKind.Object)
                    {
                        return null;
                    }

                    foreach (var property in root.EnumerateObject())
                    {
                        if (!string.Equals(property.Name, "results", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(property.Name, "evaluationResults", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (property.Value.ValueKind != JsonValueKind.Array)
                        {
                            return null;
                        }

                        return JsonSerializer.Deserialize<List<EvaluationResult>>(property.Value.GetRawText(), new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
                catch
                {
                    // ignore
                }

                return null;
            }
            set => EvaluationResultsJson = value == null
                   ? null
                   : JsonSerializer.Serialize(value);
        }

        public bool IsEvaluationCompleted { get; set; } = false;

        public string? Transcript { get; set; }

        /// <summary>
        /// Serialized JSON string of Excalidraw whiteboard elements.
        /// Persisted so late-joiners and server restarts can restore the board.
        /// </summary>
        public string? WhiteboardElements { get; set; }

        public List<QuestionItem>? QuestionList { get; set; }

        // Navigation Properties
        public InterviewBookingTransaction? Transaction { get; set; }

        public CoachAvailability? CurrentAvailability { get; set; }

        public BookingRequest? BookingRequest { get; set; }

        public CoachInterviewService? CoachInterviewService { get; set; }

        public ICollection<InterviewRescheduleRequest>? RescheduleRequests { get; set; }

        public ICollection<GeneratedQuestion> GeneratedQuestions { get; set; } = new List<GeneratedQuestion>();

        public bool IsAvailableForReschedule()
        {
            if (Status != InterviewRoomStatus.Scheduled) return false;
            if (ScheduledTime == null) return false;

            var timeUntilInterview = ScheduledTime.Value - DateTime.UtcNow;
            if (timeUntilInterview <= TimeSpan.Zero)
            {
                return false;
            }
            if (timeUntilInterview < TimeSpan.FromHours(12))
            {
                return false;
            }
            // Limit 1 reschedule attempt per interview
            if (RescheduleAttemptCount >= 1)
            {
                return false;
            }
            return true;
        }

        public bool IsAvailableForCancel()
        {
            if (Status != InterviewRoomStatus.Scheduled) return false;
            if (ScheduledTime == null) return false;

            var timeUntilInterview = ScheduledTime.Value - DateTime.UtcNow;
            // Cannot cancel if the scheduled time has already passed
            if (timeUntilInterview <= TimeSpan.Zero)
            {
                return false;
            }
            return true;
        }
    }
}
