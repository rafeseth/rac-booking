using RacBooking.Application.DTOs;
using RacBooking.Application.Queries.Public.GetPublicSalon;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RacBooking.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicBookingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicBookingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Loads tenant branding and catalog for the public booking flow (no auth).</summary>
    [HttpGet("salon/{slug}")]
    [ProducesResponseType(typeof(PublicSalonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSalon(string slug, CancellationToken cancellationToken)
    {
        var salon = await _mediator.Send(new GetPublicSalonQuery(slug), cancellationToken);
        if (salon == null)
            return NotFound();

        return Ok(salon);
    }
}
