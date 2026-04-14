using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetNeedsAttentionQueue : IGetNeedsAttentionQueue
    {
        private readonly IInterviewReportRepository _interviewReportRepository;
        private readonly IQuestionReportRepository _questionReportRepository;

        public GetNeedsAttentionQueue(
            IInterviewReportRepository interviewReportRepository,
            IQuestionReportRepository questionReportRepository)
        {
            _interviewReportRepository = interviewReportRepository;
            _questionReportRepository = questionReportRepository;
        }

        public async Task<List<NeedsAttentionItemDto>> ExecuteAsync()
        {
            var interviewReports = await _interviewReportRepository.GetPagedAsync(1, 10, InterviewReportStatus.Pending);
            var questionReports = await _questionReportRepository.GetPagedAsync(1, 10, QuestionReportStatus.Pending);

            var result = new List<NeedsAttentionItemDto>();

            foreach (var r in interviewReports.Items)
            {
                result.Add(new NeedsAttentionItemDto
                {
                    Id = r.Id,
                    Type = "Room Dispute",
                    EntityName = $"Room {r.InterviewRoomId.ToString()[..8]}...",
                    Severity = "High",
                    TimeOffset = GetTimeOffset(r.CreatedAt),
                    ActionLink = $"/admin/reports/room/{r.Id}"
                });
            }

            foreach (var r in questionReports.Items)
            {
                result.Add(new NeedsAttentionItemDto
                {
                    Id = r.Id,
                    Type = "Question Report",
                    EntityName = r.Question?.Title ?? "Unknown Question",
                    Severity = "Medium",
                    TimeOffset = GetTimeOffset(r.CreatedAt),
                    ActionLink = $"/admin/question-reports?id={r.Id}"   // TODO: replace with real link
                });
            }

            return result.OrderByDescending(x => x.Severity == "High")
                        .ThenByDescending(x => x.Id)
                        .ToList();
        }

        private string GetTimeOffset(DateTime createdAt)
        {
            var diff = DateTime.UtcNow - createdAt;
            if (diff.TotalDays >= 1) return $"{(int)diff.TotalDays}d ago";
            if (diff.TotalHours >= 1) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalMinutes >= 1) return $"{(int)diff.TotalMinutes}m ago";
            return "Just now";
        }
    }
}
