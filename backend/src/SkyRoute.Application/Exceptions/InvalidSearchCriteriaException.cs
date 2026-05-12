namespace SkyRoute.Application.Exceptions;

public class InvalidSearchCriteriaException : Exception
{
    public string Field { get; }

    public InvalidSearchCriteriaException(string field, string message) : base(message)
    {
        Field = field;
    }
}
