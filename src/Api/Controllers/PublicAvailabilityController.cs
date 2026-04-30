using System.Globalization;
using RacBooking.Application.DTOs;
using RacBooking.Application.Common.Interfaces;
using RacBooking.Application.Interfaces;
using RacBooking.Application.Queries.Availability.GetAvailableSlots;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RacBooking.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicAvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantSlugResolver _slugResolver;
    private readonly ITenantProvider _tenantProvider;

    public PublicAvailabilityController(
        IMediator mediator,
        ITenantSlugResolver slugResolver,
        ITenantProvider tenantProvider)
    {
        _mediator = mediator;
        _slugResolver = slugResolver;
        _tenantProvider = tenantProvider;
    }

    /// <summary>Available slots for a professional, service, and local calendar date (yyyy-MM-dd). Use <paramref name="slug"/> or <c>X-Tenant-Id</c>.</summary>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(IReadOnlyList<AvailableSlotResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] string? slug,
        [FromQuery] Guid professionalId,
        [FromQuery] Guid serviceId,
        [FromQuery] string date,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var tenant = await _slugResolver.GetBySlugAsync(slug.Trim(), cancellationToken);
            if (tenant == null)
                return NotFound();
            HttpContext.Items["TenantIdOverride"] = tenant.Id;
        }
        else if (_tenantProvider.TenantId == Guid.Empty)
        {
            return BadRequest(new { message = "Provide slug query parameter or X-Tenant-Id header." });
        }

        if (string.IsNullOrWhiteSpace(date) ||
            !DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateParsed))
            return BadRequest(new { message = "Invalid date. Use yyyy-MM-dd." });

        try
        {
            var slots = await _mediator.Send(
                new GetAvailableSlotsQuery(professionalId, dateParsed, serviceId),
                cancellationToken);
            return Ok(slots);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
