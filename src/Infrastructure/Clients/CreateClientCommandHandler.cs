using RacBooking.Application.Commands.Clients.CreateClient;
using RacBooking.Application.Common;
using RacBooking.Application.DTOs;
using RacBooking.Application.Exceptions;
using RacBooking.Domain.Entities;
using RacBooking.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace RacBooking.Infrastructure.Clients;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientResponse>
{
    private readonly AppDbContext _context;

    public CreateClientCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ClientResponse> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var phoneNormalized = PhoneNumberHelper.Normalize(request.Phone)
            ?? throw new AppointmentValidationException(
                "Invalid phone. Use an international-style number (for example +15550000000): 8–15 digits after +.");

        var tenantId = _context.CurrentTenantId;
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant context is required.");

        var existing = await _context.Clients
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Phone == phoneNormalized, cancellationToken);

        if (existing != null)
        {
            existing.Name = request.Name.Trim();
            if (!string.IsNullOrWhiteSpace(request.Email))
                existing.Email = request.Email.Trim();
            if (request.BirthDate.HasValue)
                existing.BirthDate = request.BirthDate;
            if (!string.IsNullOrWhiteSpace(request.AddressLine))
                existing.AddressLine = request.AddressLine.Trim();
            if (!string.IsNullOrWhiteSpace(request.Neighborhood))
                existing.Neighborhood = request.Neighborhood.Trim();
            if (!string.IsNullOrWhiteSpace(request.AddressReference))
                existing.AddressReference = request.AddressReference.Trim();

            await _context.SaveChangesAsync(cancellationToken);
            return await MapResponse(existing.Id, cancellationToken);
        }

        var client = new Client
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Phone = phoneNormalized,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            BirthDate = request.BirthDate,
            AddressLine = string.IsNullOrWhiteSpace(request.AddressLine) ? null : request.AddressLine.Trim(),
            Neighborhood = string.IsNullOrWhiteSpace(request.Neighborhood) ? null : request.Neighborhood.Trim(),
            AddressReference = string.IsNullOrWhiteSpace(request.AddressReference) ? null : request.AddressReference.Trim()
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapResponse(client.Id, cancellationToken);
    }

    private async Task<ClientResponse> MapResponse(Guid id, CancellationToken cancellationToken)
    {
        var c = await _context.Clients.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        return new ClientResponse(
            c.Id,
            c.Name,
            c.Phone,
            c.Email,
            c.BirthDate,
            c.AddressLine,
            c.Neighborhood,
            c.AddressReference);
    }
}
