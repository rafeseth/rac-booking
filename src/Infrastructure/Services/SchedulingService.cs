using System.Globalization;
using RacBooking.Application.Common.Interfaces;
using RacBooking.Application.DTOs;
using RacBooking.Application.Exceptions;
using RacBooking.Application.Interfaces;
using RacBooking.Domain.Entities;
using RacBooking.Domain.Enums;
using RacBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RacBooking.Infrastructure.Services;

/// <summary>Availability engine: working hours, service duration + buffer, appointments, and schedule blocks.</summary>
public sealed class SchedulingService : ISchedulingService
{
    private const int SlotStepMinutes = 15;
    private const int PerfectFitThresholdMinutes = 5;

    private readonly AppDbContext _context;
    private readonly ICurrentTenantTimeZoneProvider _tenantTimeZone;
    private readonly ICurrentTenantPortalUser _portal;

    public SchedulingService(
        AppDbContext context,
        ICurrentTenantTimeZoneProvider tenantTimeZone,
        ICurrentTenantPortalUser portal)
    {
        _context = context;
        _tenantTimeZone = tenantTimeZone;
        _portal = portal;
    }

    private static DateTime ToUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc) return value;
        if (value.Kind == DateTimeKind.Local) return value.ToUniversalTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static bool IntervalsOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd) =>
        aStart < bEnd && aEnd > bStart;

    private static DateTime RoundUpToNextSlotGrid(DateTime value, int stepMinutes)
    {
        var totalMinutes = (int)value.TimeOfDay.TotalMinutes;
        var remainder = totalMinutes % stepMinutes;
        var addMinutes = remainder == 0 ? 0 : stepMinutes - remainder;
        return value.AddMinutes(addMinutes);
    }

    private static DateTime SalonLocalToUtc(DateTime dateOnly, TimeSpan localTimeOfDay, string timeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var localDateTime = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 0, 0, 0, DateTimeKind.Unspecified)
            .Add(localTimeOfDay);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, tz);
    }

    private static (string DisplayTime, string DisplayDate) ToSalonLocalDisplay(DateTime utc, string timeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        return (
            local.ToString("HH:mm", CultureInfo.InvariantCulture),
            local.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
    }

    private void EnsureProfessionalScope(Guid professionalId)
    {
        if (!_portal.IsProfessionalRestricted) return;
        if (_portal.LinkedProfessionalId != professionalId)
            throw new InvalidOperationException("Not allowed to query availability for this professional.");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AvailableSlotResponse>> GetAvailableSlotsAsync(
        Guid professionalId,
        DateTime date,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        EnsureProfessionalScope(professionalId);

        var tenantId = _context.CurrentTenantId;
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant context is required.");

        var tzId = await _tenantTimeZone.GetTimeZoneIdAsync(cancellationToken);
        var salonTz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
        var nowSalon = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, salonTz);
        var isSelectedDayTodayInSalonTz = date.Date == nowSalon.Date;

        var dayOfWeek = date.Date.DayOfWeek;
        var dayStart = ToUtc(date.Date);
        var dayEnd = dayStart.AddDays(1);

        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);
        if (service == null)
            throw new InvalidOperationException("Service not found.");

        var canPerform = await _context.ProfessionalServices
            .AnyAsync(
                ps => ps.ProfessionalId == professionalId && ps.ServiceId == serviceId,
                cancellationToken);

        var durationMinutes = service.DurationMinutes;
        var bufferAfter = service.BufferAfterMinutes;
        if (durationMinutes <= 0)
            throw new InvalidOperationException("Service duration must be greater than zero.");

        var totalBlockMinutes = durationMinutes + bufferAfter;

        var workingHours = await _context.ProfessionalWorkingHours
            .AsNoTracking()
            .Where(h => h.ProfessionalId == professionalId && h.DayOfWeek == dayOfWeek)
            .ToListAsync(cancellationToken);

        if (!canPerform || workingHours.Count == 0)
            return Array.Empty<AvailableSlotResponse>();

        var appointmentsWithService = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Where(a =>
                a.ProfessionalId == professionalId &&
                a.StartTime >= dayStart &&
                a.StartTime < dayEnd &&
                a.Status != AppointmentStatus.Canceled)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        var scheduleBlocks = await _context.ScheduleBlocks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(b =>
                b.TenantId == tenantId &&
                b.ProfessionalId == professionalId &&
                b.StartTime < dayEnd &&
                b.EndTime > dayStart)
            .ToListAsync(cancellationToken);

        var rawBlocked = appointmentsWithService
            .Select(a => (
                Start: a.StartTime,
                End: a.EndTime.AddMinutes(a.Service?.BufferAfterMinutes ?? 0)))
            .Concat(scheduleBlocks.Select(b => (Start: b.StartTime, End: b.EndTime)))
            .OrderBy(x => x.Start)
            .ToList();

        var blockedIntervals = MergeOverlappingIntervals(rawBlocked);

        var slots = new List<AvailableSlotResponse>();

        foreach (var wh in workingHours)
        {
            var periodStart = SalonLocalToUtc(date.Date, wh.StartTime, tzId);
            var periodEnd = SalonLocalToUtc(date.Date, wh.EndTime, tzId);

            var periodBlocked = blockedIntervals
                .Where(b => b.End > periodStart && b.Start < periodEnd)
                .Select(b => (
                    Start: b.Start < periodStart ? periodStart : b.Start,
                    End: b.End > periodEnd ? periodEnd : b.End))
                .OrderBy(b => b.Start)
                .ToList();

            var gaps = BuildGaps(periodStart, periodEnd, periodBlocked);

            foreach (var (gapStart, gapEnd) in gaps)
            {
                var slotStart = RoundUpToNextSlotGrid(gapStart, SlotStepMinutes);
                while (slotStart.AddMinutes(totalBlockMinutes) <= gapEnd)
                {
                    var slotEnd = slotStart.AddMinutes(durationMinutes);
                    var blockedUntil = slotStart.AddMinutes(totalBlockMinutes);
                    var gapAfterMinutes = (gapEnd - blockedUntil).TotalMinutes;

                    var priority = gapAfterMinutes is >= 0 and <= PerfectFitThresholdMinutes
                        ? SlotPriority.PerfectFit
                        : gapAfterMinutes > PerfectFitThresholdMinutes && gapAfterMinutes < durationMinutes
                            ? SlotPriority.CreatesSmallGap
                            : SlotPriority.Preferred;

                    var (displayTime, displayDate) = ToSalonLocalDisplay(slotStart, tzId);
                    slots.Add(new AvailableSlotResponse(slotStart, slotEnd, displayTime, displayDate, priority));
                    slotStart = slotStart.AddMinutes(SlotStepMinutes);
                }
            }
        }

        foreach (var block in scheduleBlocks)
            slots.RemoveAll(s => IntervalsOverlap(s.StartTimeUtc, s.EndTimeUtc, block.StartTime, block.EndTime));

        if (isSelectedDayTodayInSalonTz)
        {
            slots.RemoveAll(s =>
            {
                var slotStartSalon = TimeZoneInfo.ConvertTimeFromUtc(s.StartTimeUtc, salonTz);
                return slotStartSalon <= nowSalon;
            });
        }

        return slots
            .OrderBy(s => s.StartTimeUtc)
            .ThenBy(s => s.Priority)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<AppointmentResponse> CreateAppointmentAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default,
        Guid? tenantIdForBlockCheck = null)
    {
        EnsureProfessionalScope(request.ProfessionalId);

        var tenantId = _context.CurrentTenantId;
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant context is required.");

        var blockTenantId = tenantIdForBlockCheck ?? tenantId;
        var startUtc = ToUtc(request.StartTime);
        var tzId = await _tenantTimeZone.GetTimeZoneIdAsync(cancellationToken);
        var salonTz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
        var nowSalon = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, salonTz);
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, salonTz);
        if (localStart <= nowSalon)
            throw new AppointmentValidationException("Start time must be in the future.");

        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);
        if (service == null || !service.IsActive)
            throw new AppointmentValidationException("Service not found or inactive.");

        var professional = await _context.Professionals
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProfessionalId, cancellationToken);
        if (professional == null || !professional.IsActive)
            throw new AppointmentValidationException("Professional not found or inactive.");

        var canPerform = await _context.ProfessionalServices
            .AnyAsync(
                ps => ps.ProfessionalId == request.ProfessionalId && ps.ServiceId == request.ServiceId,
                cancellationToken);
        if (!canPerform)
            throw new AppointmentValidationException("This professional does not offer the selected service.");

        var durationMinutes = service.DurationMinutes;
        var bufferAfter = service.BufferAfterMinutes;
        if (durationMinutes <= 0)
            throw new AppointmentValidationException("Service duration must be greater than zero.");

        var newBlockEndUtc = startUtc.AddMinutes(durationMinutes + bufferAfter);

        if (!await IsIntervalWithinWorkingHoursAsync(
                request.ProfessionalId,
                startUtc,
                newBlockEndUtc,
                tzId,
                cancellationToken))
        {
            var alt = await GetAvailableSlotsAsync(
                request.ProfessionalId,
                localStart.Date,
                request.ServiceId,
                cancellationToken);
            throw new AppointmentConflictException(
                "The chosen time is outside working hours or no longer available.",
                alt.Take(20).ToList());
        }

        if (await HasActiveAppointmentOverlapAsync(
                request.ProfessionalId,
                startUtc,
                newBlockEndUtc,
                cancellationToken))
        {
            var alt = await GetAvailableSlotsAsync(
                request.ProfessionalId,
                localStart.Date,
                request.ServiceId,
                cancellationToken);
            throw new AppointmentConflictException(
                "The chosen time conflicts with another appointment.",
                alt.Take(20).ToList());
        }

        if (await HasScheduleBlockOverlapAsync(
                request.ProfessionalId,
                startUtc,
                newBlockEndUtc,
                blockTenantId,
                cancellationToken))
        {
            var alt = await GetAvailableSlotsAsync(
                request.ProfessionalId,
                localStart.Date,
                request.ServiceId,
                cancellationToken);
            throw new AppointmentConflictException(
                "The chosen time conflicts with a schedule block.",
                alt.Take(20).ToList());
        }

        var engineSlots = await GetAvailableSlotsAsync(
            request.ProfessionalId,
            localStart.Date,
            request.ServiceId,
            cancellationToken);
        if (engineSlots.All(s => s.StartTimeUtc != startUtc))
        {
            throw new AppointmentConflictException(
                "The chosen time is not available. Pick a slot from availability.",
                engineSlots.Take(20).ToList());
        }

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (await HasActiveAppointmentOverlapAsync(
                    request.ProfessionalId,
                    startUtc,
                    newBlockEndUtc,
                    cancellationToken))
            {
                var alt = await GetAvailableSlotsAsync(
                    request.ProfessionalId,
                    localStart.Date,
                    request.ServiceId,
                    cancellationToken);
                throw new AppointmentConflictException(
                    "The chosen time was just taken. Please pick another slot.",
                    alt.Take(20).ToList());
            }

            if (await HasScheduleBlockOverlapAsync(
                    request.ProfessionalId,
                    startUtc,
                    newBlockEndUtc,
                    blockTenantId,
                    cancellationToken))
            {
                var alt = await GetAvailableSlotsAsync(
                    request.ProfessionalId,
                    localStart.Date,
                    request.ServiceId,
                    cancellationToken);
                throw new AppointmentConflictException(
                    "The chosen time conflicts with a schedule block.",
                    alt.Take(20).ToList());
            }

            var endUtc = startUtc.AddMinutes(durationMinutes);
            var price = service.Price;
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClientId = request.ClientId,
                ProfessionalId = request.ProfessionalId,
                ServiceId = request.ServiceId,
                StartTime = startUtc,
                EndTime = endUtc,
                Status = AppointmentStatus.Scheduled,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                OriginalServicePrice = price,
                FinalServicePrice = price,
                ServicePrice = price,
                AttendanceLocationType = request.AttendanceLocationType,
                ClientAddress = request.ClientAddress,
                ClientNeighborhood = request.ClientNeighborhood,
                ClientAddressReference = request.ClientAddressReference
            };

            _context.Appointments.Add(appointment);

            if (request.AttendanceLocationType == AttendanceLocationType.ClientAddress)
            {
                var client = await _context.Clients.FirstOrDefaultAsync(
                    c => c.Id == request.ClientId,
                    cancellationToken);
                if (client != null)
                {
                    if (!string.IsNullOrWhiteSpace(request.ClientAddress))
                        client.AddressLine = request.ClientAddress;
                    if (!string.IsNullOrWhiteSpace(request.ClientNeighborhood))
                        client.Neighborhood = request.ClientNeighborhood;
                    if (!string.IsNullOrWhiteSpace(request.ClientAddressReference))
                        client.AddressReference = request.ClientAddressReference;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return MapToResponse(appointment);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static AppointmentResponse MapToResponse(Appointment a) =>
        new(
            a.Id,
            a.ClientId,
            a.ProfessionalId,
            a.ServiceId,
            a.StartTime,
            a.EndTime,
            a.Status,
            a.Notes,
            a.ServicePrice,
            a.OriginalServicePrice,
            a.FinalServicePrice,
            a.AttendanceLocationType,
            a.ClientAddress,
            a.ClientNeighborhood,
            a.ClientAddressReference);

    private async Task<bool> IsIntervalWithinWorkingHoursAsync(
        Guid professionalId,
        DateTime startUtc,
        DateTime blockEndUtc,
        string timeZoneId,
        CancellationToken cancellationToken)
    {
        var salonTz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, salonTz);
        var dayOfWeek = localStart.DayOfWeek;
        var localDate = localStart.Date;

        var workingHours = await _context.ProfessionalWorkingHours
            .AsNoTracking()
            .Where(h => h.ProfessionalId == professionalId && h.DayOfWeek == dayOfWeek)
            .ToListAsync(cancellationToken);

        if (workingHours.Count == 0)
            return false;

        foreach (var wh in workingHours)
        {
            var periodStart = SalonLocalToUtc(localDate, wh.StartTime, timeZoneId);
            var periodEnd = SalonLocalToUtc(localDate, wh.EndTime, timeZoneId);
            if (startUtc >= periodStart && blockEndUtc <= periodEnd)
                return true;
        }

        return false;
    }

    private async Task<bool> HasActiveAppointmentOverlapAsync(
        Guid professionalId,
        DateTime startUtc,
        DateTime newBlockEndUtc,
        CancellationToken cancellationToken)
    {
        var from = startUtc.AddDays(-1);
        var to = newBlockEndUtc.AddDays(1);
        var apps = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Where(a =>
                a.ProfessionalId == professionalId &&
                a.Status != AppointmentStatus.Canceled &&
                a.StartTime < to &&
                a.EndTime > from)
            .ToListAsync(cancellationToken);

        foreach (var a in apps)
        {
            var buf = a.Service?.BufferAfterMinutes ?? 0;
            var endWithBuf = a.EndTime.AddMinutes(buf);
            if (IntervalsOverlap(startUtc, newBlockEndUtc, a.StartTime, endWithBuf))
                return true;
        }

        return false;
    }

    private async Task<bool> HasScheduleBlockOverlapAsync(
        Guid professionalId,
        DateTime startUtc,
        DateTime newBlockEndUtc,
        Guid blockTenantId,
        CancellationToken cancellationToken) =>
        await _context.ScheduleBlocks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(
                b =>
                    b.TenantId == blockTenantId &&
                    b.ProfessionalId == professionalId &&
                    b.StartTime < newBlockEndUtc &&
                    b.EndTime > startUtc,
                cancellationToken);

    private static List<(DateTime Start, DateTime End)> MergeOverlappingIntervals(List<(DateTime Start, DateTime End)> ordered)
    {
        if (ordered.Count == 0) return ordered;
        var merged = new List<(DateTime Start, DateTime End)> { ordered[0] };
        for (var i = 1; i < ordered.Count; i++)
        {
            var prev = merged[^1];
            var curr = ordered[i];
            if (curr.Start < prev.End)
                merged[^1] = (prev.Start, prev.End > curr.End ? prev.End : curr.End);
            else
                merged.Add(curr);
        }
        return merged;
    }

    private static List<(DateTime Start, DateTime End)> BuildGaps(
        DateTime periodStart,
        DateTime periodEnd,
        List<(DateTime Start, DateTime End)> periodBlocked)
    {
        var gaps = new List<(DateTime Start, DateTime End)>();
        if (periodBlocked.Count == 0)
        {
            gaps.Add((periodStart, periodEnd));
            return gaps;
        }

        if (periodBlocked[0].Start > periodStart)
            gaps.Add((periodStart, periodBlocked[0].Start));

        for (var i = 0; i < periodBlocked.Count - 1; i++)
            gaps.Add((periodBlocked[i].End, periodBlocked[i + 1].Start));

        if (periodBlocked[^1].End < periodEnd)
            gaps.Add((periodBlocked[^1].End, periodEnd));

        return gaps;
    }
}
