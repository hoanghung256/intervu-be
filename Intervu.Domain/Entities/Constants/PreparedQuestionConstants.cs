namespace Intervu.Domain.Entities.Constants.PreparedQuestionConstants
{
    /// <summary>
    /// Drives the in-room action button for a prepared question.
    /// NonCoding -> "Mark as Asked" (no candidate broadcast).
    /// Coding    -> "Send to Editor" (broadcasts ReceiveProblem to the candidate).
    /// </summary>
    public enum PreparedQuestionInteractionType
    {
        NonCoding = 1,
        Coding = 2
    }

    public enum PreparedQuestionStatus
    {
        Pending = 1,
        Asked = 2
    }
}
