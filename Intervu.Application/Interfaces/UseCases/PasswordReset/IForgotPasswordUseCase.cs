using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.PasswordReset;

namespace Intervu.Application.Interfaces.UseCases.PasswordReset
{
    public interface IForgotPasswordUseCase
    {
        Task<PasswordResetResponse> ExecuteAsync(ForgotPasswordRequest request);
    }
}
