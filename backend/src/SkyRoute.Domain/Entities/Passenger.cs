namespace SkyRoute.Domain.Entities;

using SkyRoute.Domain.Enums;

public class Passenger
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public DocumentType DocumentType { get; init; }
}
