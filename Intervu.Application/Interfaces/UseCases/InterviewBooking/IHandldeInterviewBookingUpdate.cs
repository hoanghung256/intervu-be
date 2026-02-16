using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore;

namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    public interface IHandldeInterviewBookingUpdate
    {
        /// <summary>
        /// Processes a PayOS webhook payload to verify payment status.
        /// If the payment is successful, updates the interview booking transaction
        /// and marks the corresponding coach availability as booked.
        /// </summary>
        /// <param name="webhookPayload">
        /// The webhook payload received from PayOS containing payment information.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        Task ExecuteAsync(object webhookPayload);

    }
}
