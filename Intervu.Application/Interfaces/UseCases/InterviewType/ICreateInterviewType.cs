using Intervu.Application.DTOs.InterviewType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewType
{
    public interface ICreateInterviewType
    {
        Task ExecuteAsync(InterviewTypeDto interviewType);
    }
}
