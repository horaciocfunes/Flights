namespace SkyRoute.API.DTOs;

using System.ComponentModel.DataAnnotations;
using SkyRoute.Domain.Enums;

public class FlightSearchRequestDto : IValidatableObject
{
    [Required] public string OriginCode { get; init; } = string.Empty;
    [Required] public string DestinationCode { get; init; } = string.Empty;
    [Required] public DateTime DepartureDate { get; init; }
    [Range(1, 9)] public int PassengerCount { get; init; } = 1;
    public CabinClass CabinClass { get; init; } = CabinClass.Economy;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DepartureDate.Date < DateTime.UtcNow.Date)
            yield return new ValidationResult(
                "Departure date cannot be in the past.",
                new[] { nameof(DepartureDate) });
    }
}
