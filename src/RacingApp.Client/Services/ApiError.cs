namespace RacingApp.Client.Services;

public class ApiError : Exception
{
    public ApiError(string message) : base(message)
    {
    }
}
