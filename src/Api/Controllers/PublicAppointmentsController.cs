using RacBooking.Application.Commands.Appointments.CreateAppointment;
using RacBooking.Application.Commands.Clients.CreateClient;
using RacBooking.Application.Common;
using RacBooking.Application.Common.Interfaces;
using RacBooking.Application.DTOs;
using RacBooking.Domain.Entities;
using RacBooking.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RacBooking.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicAppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantSlugResolver _slugResolver;

    public PublicAppointmentsController(IMediator mediator, ITenantSlugResolver slugResolver)
    {
        _mediator = mediator;
        _slugResolver = slugResolver;
    }

    /// <summary>Creates an appointment for an anonymous client. Requires <paramref name="slug"/> (tenant).</summary>
    [HttpPost("appointments")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromQuery] string? slug,
        [FromBody] PublicCreateAppointmentRequest body,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest(new { message = "Provide slug query parameter (tenant public slug)." });

        var tenant = await _slugResolver.GetBySlugAsync(slug.Trim(), cancellationToken);
        if (tenant == null)
            return NotFound();

        HttpContext.Items["TenantIdOverride"] = tenant.Id;

        if (string.IsNullOrWhiteSpace(body.Name))
            return BadRequest(new { message = "Name is required." });

        if (!TryResolveAttendance(tenant, body, out var attendanceType, out var addr, out var nbh, out var reference, out var errorMessage))
            return BadRequest(new { message = errorMessage });

        var client = await _mediator.Send(
            new CreateClientCommand(
                body.Name,
                body.Phone,
                body.Email,
                null,
                attendanceType == AttendanceLocationType.ClientAddress ? body.ClientAddress : null,
                attendanceType == AttendanceLocationType.ClientAddress ? body.ClientNeighborhood : null,
                attendanceType == AttendanceLocationType.ClientAddress ? body.ClientAddressReference : null),
            cancellationToken);

        var createRequest = new CreateAppointmentRequest(
            client.Id,
            body.ProfessionalId,
            body.ServiceId,
            body.StartTime,
            body.Notes,
            attendanceType,
            addr,
            nbh,
            reference);

        var appointment = await _mediator.Send(
            new CreateAppointmentCommand(createRequest, tenant.Id),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, appointment);
    }

    private static bool TryResolveAttendance(
        Tenant tenant,
        PublicCreateAppointmentRequest req,
        out AttendanceLocationType attendanceType,
        out string? clientAddress,
        out string? clientNeighborhood,
        out string? clientAddressReference,
        out string? errorMessage)
    {
        attendanceType = AttendanceLocationType.Salon;
        clientAddress = null;
        clientNeighborhood = null;
        clientAddressReference = null;
        errorMessage = null;

        var mode = tenant.AttendanceMode;
        var requested = req.AttendanceLocationType;

        if (mode == AttendanceMode.Salon)
        {
            if (requested == AttendanceLocationType.ClientAddress)
            {
                errorMessage = "This salon only accepts appointments at the business address.";
                return false;
            }

            return true;
        }

        if (mode == AttendanceMode.ClientAddress)
        {
            if (requested == AttendanceLocationType.Salon)
            {
                errorMessage = "This business only offers service at the client's address.";
                return false;
            }

            return TryNormalizeDomicile(
                req,
                out attendanceType,
                out clientAddress,
                out clientNeighborhood,
                out clientAddressReference,
                out errorMessage);
        }

        attendanceType = requested ?? AttendanceLocationType.Salon;
        if (attendanceType == AttendanceLocationType.Salon)
            return true;

        return TryNormalizeDomicile(
            req,
            out attendanceType,
            out clientAddress,
            out clientNeighborhood,
            out clientAddressReference,
            out errorMessage);
    }

    private static bool TryNormalizeDomicile(
        PublicCreateAppointmentRequest req,
        out AttendanceLocationType attendanceType,
        out string? clientAddress,
        out string? clientNeighborhood,
        out string? clientAddressReference,
        out string? errorMessage)
    {
        attendanceType = AttendanceLocationType.ClientAddress;
        clientAddress = null;
        clientNeighborhood = null;
        clientAddressReference = null;
        errorMessage = null;

        if (!DomicileAddressValidation.TryNormalizeAddressLine(req.ClientAddress, out var addr, out var err))
        {
            errorMessage = err;
            return false;
        }

        if (!DomicileAddressValidation.TryNormalizeNeighborhood(req.ClientNeighborhood, out var nbh, out err))
        {
            errorMessage = err;
            return false;
        }

        if (!DomicileAddressValidation.TryNormalizeReference(req.ClientAddressReference, out var reference, out err))
        {
            errorMessage = err;
            return false;
        }

        clientAddress = addr;
        clientNeighborhood = nbh;
        clientAddressReference = reference;
        return true;
    }
}
