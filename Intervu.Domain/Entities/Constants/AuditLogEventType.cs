namespace Intervu.Domain.Entities.Constants
{
    public enum AuditLogEventType
    {
        RoomJoin = 0,
        RoomLeave = 1,
        RoomDisconnect = 2,
        CodeRun = 3,
        LanguageChange = 4,
        ProblemUpdate = 5,
        CameraToggle = 6,
        MicToggle = 7,
        InterviewProblemReport = 8
    }
}
