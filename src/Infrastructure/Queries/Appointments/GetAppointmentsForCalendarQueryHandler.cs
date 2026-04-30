using RacBooking.Application.Common.Interfaces;
using RacBooking.Application.DTOs;
using RacBooking.Application.Queries.Appointments.GetAppointmentsForCalendar;
using RacBooking.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace RacBooking.Infrastructure.Queries.Appointments;

public sealed class GetAppointmentsForCalendarQueryHandler
    : IRequestHandler<GetAppointmentsForCalendarQuery, IReadOnlyList<AppointmentCalendarItemResponse>>
{
    private readonly AppDbContext _context;
    private readonly ICurrentTenantTimeZoneProvider _timeZone;

    public GetAppointmentsForCalendarQueryHandler(
        AppDbContext context,
        ICurrentTenantTimeZoneProvider timeZone)
    {
        _context = context;
        _timeZone = timeZone;
    }

    public async Task<IReadOnlyList<AppointmentCalendarItemResponse>> Handle(
        GetAppointmentsForCalendarQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _context.CurrentTenantId;
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant context is required.");

        var tzId = await _timeZone.GetTimeZoneIdAsync(cancellationToken);
        var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);

        var localRangeStart = DateTime.SpecifyKind(
            request.From.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Unspecified);
        var localRangeEndExclusive = DateTime.SpecifyKind(
            request.To.AddDays(1).ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Unspecified);

        var rangeStartUtc = TimeZoneInfo.ConvertTimeToUtc(localRangeStart, tz);
        var rangeEndUtcExclusive = TimeZoneInfo.ConvertTimeToUtc(localRangeEndExclusive, tz);

        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Include(a => a.Professional)
            .Include(a => a.Client)
            .Where(a =>
                a.StartTime < rangeEndUtcExclusive &&
                a.EndTime > rangeStartUtc);

        if (request.ProfessionalId is { } pid)
            query = query.Where(a => a.ProfessionalId == pid);

        var rows = await query
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        return rows.Select(a =>
        {
            var serviceName = a.Service?.Name ?? "Service";
            var professionalName = a.Professional?.Name ?? "Professional";
            var clientName = a.Client?.Name ?? "Client";
            var title = $"{serviceName} - {clientName}";
            return new AppointmentCalendarItemResponse(
                a.Id,
                title,
                a.StartTime,
                a.EndTime,
                a.Status,
                serviceName,
                professionalName,
                clientName,
                a.ServicePrice);
        }).ToList();
    }
}
