namespace Intervu.Domain.Entities.Constants
{
    public static class NotificationType
    {
        // Booking
        public const string BOOKING_NEW = "BOOKING_NEW";
        public const string BOOKING_ACCEPTED = "BOOKING_ACCEPTED";
        public const string BOOKING_REJECTED = "BOOKING_REJECTED";
        public const string PAYMENT_SUCCESS = "PAYMENT_SUCCESS";

        // Reschedule
        public const string RESCHEDULE_REQUESTED = "RESCHEDULE_REQUESTED";
        public const string RESCHEDULE_ACCEPTED = "RESCHEDULE_ACCEPTED";
        public const string RESCHEDULE_REJECTED = "RESCHEDULE_REJECTED";

        // Interview Room
        public const string INTERVIEW_REMINDER = "INTERVIEW_REMINDER";

        // Feedback & AI
        public const string FEEDBACK_RECEIVED = "FEEDBACK_RECEIVED";
        public const string AI_ANALYSIS_COMPLETED = "AI_ANALYSIS_COMPLETED";

        // System
        public const string SYSTEM_ANNOUNCEMENT = "SYSTEM_ANNOUNCEMENT";
    }
}
