using System.Globalization;
using RacBooking.Application.DTOs;
using RacBooking.Application.Interfaces;
using RacBooking.Application.Queries.Appointments.GetAppointmentsForCalendar;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RacBooking.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantProvider _tenantProvider;

    public AppointmentsController(IMediator mediator, ITenantProvider tenantProvider)
    {
        _mediator = mediator;
        _tenantProvider = tenantProvider;
    }

    /// <summary>Admin calendar feed (tenant via <c>X-Tenant-Id</c>). Dates are interpreted in the tenant time zone.</summary>
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentCalendarItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] Guid? professionalId,
        CancellationToken cancellationToken)
    {
        if (_tenantProvider.TenantId == Guid.Empty)
            return BadRequest(new { message = "X-Tenant-Id header is required." });

        if (string.IsNullOrWhiteSpace(from) ||
            string.IsNullOrWhiteSpace(to) ||
            !DateOnly.TryParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate) ||
            !DateOnly.TryParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
            return BadRequest(new { message = "Invalid from or to. Use yyyy-MM-dd." });

        if (toDate < fromDate)
            return BadRequest(new { message = "to must be on or after from." });

        var items = await _mediator.Send(
            new GetAppointmentsForCalendarQuery(fromDate, toDate, professionalId),
            cancellationToken);

        return Ok(items);
    }
}
