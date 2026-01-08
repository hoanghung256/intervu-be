using Intervu.Domain.Entities.Constants;

namespace Intervu.API.Utils.Constant
{
    public static class AuthorizationPolicies
    {
        public const string Candidate = "Candidate";
        public const string Interviewer = "Interviewer";
        public const string Admin = "Admin";
        public const string InterviewOrAdmin = "InterviewOrAdmin";
        public const string CandidateOrAdmin = "CandidateOrAdmin";
        public const string CandidateOrInterviewer = "CandidateOrInterviewer";
    }
}
