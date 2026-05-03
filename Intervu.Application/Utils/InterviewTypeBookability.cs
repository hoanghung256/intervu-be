using Intervu.Application.Exceptions;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Utils;

/// <summary>
/// Only <see cref="InterviewTypeStatus.Active"/> interview types may be offered by coaches or selected for new bookings.
/// </summary>
public static class InterviewTypeBookability
{
    public static void EnsureActiveForCoachServices(InterviewType interviewType)
    {
        ArgumentNullException.ThrowIfNull(interviewType);
        if (interviewType.Status != InterviewTypeStatus.Active)
            throw new BadRequestException(
                $"Interview type \"{interviewType.Name}\" is not active and cannot be used to add or edit coach services.");
    }

    public static void EnsureActiveForBooking(InterviewType interviewType)
    {
        ArgumentNullException.ThrowIfNull(interviewType);
        if (interviewType.Status != InterviewTypeStatus.Active)
            throw new BadRequestException(
                $"Interview type \"{interviewType.Name}\" is not available for booking.");
    }
}
