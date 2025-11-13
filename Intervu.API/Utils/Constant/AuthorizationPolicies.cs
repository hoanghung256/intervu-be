using Intervu.Domain.Entities.Constants;

namespace Intervu.API.Utils.Constant
{
    public static class AuthorizationPolicies
    {
        public const string Interviewee = "Interviewee";
        public const string Interviewer = "Interviewer";
        public const string Admin = "Admin";
        public const string InterviewOrAdmin = "InterviewOrAdmin";
        public const string IntervieweeOrAdmin = "IntervieweeOrAdmin";
        public const string IntervieweeOrInterviewer = "IntervieweeOrInterviewer";
    }
}
