namespace SkyRoute.Application.Validators;

using System.Text.RegularExpressions;
using FluentValidation;
using SkyRoute.Application.Models;

public class BookingRequestValidator : AbstractValidator<BookingRequest>
{
    private static readonly Regex PassportRegex =
        new(@"^[A-Z0-9]{6,12}$", RegexOptions.Compiled);

    private static readonly Regex NationalIdRegex =
        new(@"^\d{7,10}$", RegexOptions.Compiled);

    public BookingRequestValidator()
    {
        RuleFor(x => x.FlightId).NotEmpty();

        RuleFor(x => x.Passengers)
            .NotEmpty()
            .WithMessage("At least one passenger is required.");

        RuleForEach(x => x.Passengers).ChildRules(passenger =>
        {
            passenger.RuleFor(p => p.FullName)
                .NotEmpty()
                .MaximumLength(100);

            passenger.RuleFor(p => p.Email)
                .NotEmpty()
                .EmailAddress();

            passenger.RuleFor(p => p.DocumentNumber)
                .NotEmpty();
        });

        // Document format and uniqueness validation requires the full request,
        // so it runs as a top-level custom rule after per-passenger rules pass.
        RuleFor(x => x).Custom((request, ctx) =>
        {
            if (request.Passengers is null) return;

            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < request.Passengers.Count; i++)
            {
                var doc = request.Passengers[i].DocumentNumber;
                if (string.IsNullOrEmpty(doc)) continue;

                var isValid = request.IsInternational
                    ? PassportRegex.IsMatch(doc)
                    : NationalIdRegex.IsMatch(doc);

                if (!isValid)
                {
                    var expected = request.IsInternational
                        ? "passport number (uppercase letters and digits, 6–12 characters)"
                        : "national ID (7–10 digits)";

                    ctx.AddFailure(
                        $"Passengers[{i}].DocumentNumber",
                        $"Document number must be a valid {expected}.");
                }

                if (seen.TryGetValue(doc, out int firstIndex))
                {
                    ctx.AddFailure(
                        $"Passengers[{i}].DocumentNumber",
                        $"Document number is already used by passenger {firstIndex + 1}.");
                }
                else
                {
                    seen[doc] = i;
                }
            }
        });
    }
}
