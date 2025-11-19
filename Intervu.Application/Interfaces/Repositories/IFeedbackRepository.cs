using Intervu.Application.Common;
using Intervu.Domain.Entities;
using System.Threading.Tasks;
﻿using Intervu.Application.DTOs.Interviewer;
using Intervu.Domain.Entities;
using Intervu.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Feedback;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface IFeedbackRepository : IRepositoryBase<Feedback>
    {
        Task<PagedResult<Feedback>> GetPagedFeedbacksAsync(int page, int pageSize);
        Task<int> GetTotalFeedbacksCountAsync();
        Task<double> GetAverageRatingAsync();
        Task<PagedResult<Feedback>> GetFeedbacksByStudentIdAsync(GetFeedbackRequest request);
        Task<Feedback?> GetFeedbackByIdAsync(int id);
        Task<List<Feedback>> GetFeedbacksByInterviewRoomIdAsync(int interviewRoomId);
        Task CreateFeedbackAsync(Feedback feedback);
        Task UpdateFeedbackAsync(Feedback updatedFeedback);
    }
}
