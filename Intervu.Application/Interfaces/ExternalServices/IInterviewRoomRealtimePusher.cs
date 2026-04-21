using Intervu.Application.DTOs.PreparedQuestion;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices
{
    /// <summary>
    /// Thin abstraction over the InterviewRoomHub SignalR broadcaster so use cases
    /// can stay decoupled from the API/Hub layer. Each method is a best-effort push
    /// to all connected clients in the given room group; failures should be logged
    /// but MUST NOT abort the calling business operation.
    /// </summary>
    public interface IInterviewRoomRealtimePusher
    {
        /// <summary>
        /// Broadcasts a status change for one prepared question to every client in
        /// the room. The candidate peer ignores it (UI-gated); the coach uses it to
        /// sync Workspace state across their own tabs/devices.
        /// </summary>
        Task PushPreparedQuestionStatusChangedAsync(Guid interviewRoomId, PreparedQuestionDto dto);

        /// <summary>
        /// Mirrors InterviewRoomHub.SendProblem: pushes the problem description,
        /// short name and test cases to the room (excluding the caller). Used by
        /// "Send to Editor" so the candidate's Monaco editor and problem description
        /// panel update in real time.
        /// </summary>
        Task PushProblemToRoomAsync(
            Guid interviewRoomId,
            Guid? excludeUserId,
            string description,
            string shortName,
            object[] testCases);
    }
}
