using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Email;

namespace Intervu.Application.Interfaces.UseCases.Email
{
    public interface ISendBookingConfirmationEmail
    {
        Task ExecuteAsync(SendBookingConfirmationEmailDto dto);
    }
}
