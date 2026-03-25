using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.InterviewRoom
{
    public class EvaluationResultDto
    {
        public string Type { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class CoachEvaluationResponseDto
    {
        public Guid InterviewRoomId { get; set; }
        public Guid? CoachId { get; set; }
        public Guid? CandidateId { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public InterviewRoomStatus Status { get; set; }
        public bool IsEvaluationCompleted { get; set; }
        public List<EvaluationResultDto> EvaluationResults { get; set; } = new();
    }

    public class SubmitCoachEvaluationRequest
    {
        public List<EvaluationResultDto> Results { get; set; } = new();
    }
}
