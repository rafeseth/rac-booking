using RacBooking.Application.DTOs;
using MediatR;

namespace RacBooking.Application.Commands.Clients.CreateClient;

public record CreateClientCommand(
    string Name,
    string Phone,
    string? Email,
    DateOnly? BirthDate = null,
    string? AddressLine = null,
    string? Neighborhood = null,
    string? AddressReference = null) : IRequest<ClientResponse>;
