using RacBooking.Application.DTOs;
using MediatR;

namespace RacBooking.Application.Queries.Public.GetPublicSalon;

public record GetPublicSalonQuery(string Slug) : IRequest<PublicSalonResponse?>;
