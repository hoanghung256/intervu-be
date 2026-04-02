namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IScheduleInterviewReminders
    {
        /// <summary>
        /// Schedules 4 reminder notifications (1 day, 12 hours, 1 hour, and 5 minutes
        /// before the interview) for both the candidate and coach.
        /// Only milestones that have not yet passed will be scheduled.
        /// </summary>
        void Schedule(Guid roomId, DateTime scheduledTime);
    }
}
