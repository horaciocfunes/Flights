namespace SkyRoute.Application.Exceptions;

using FluentValidation.Results;

public class BookingValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public BookingValidationException(IEnumerable<ValidationFailure> errors)
        : base("One or more booking validation errors occurred.")
    {
        Errors = errors;
    }
}
