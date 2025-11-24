
namespace Backend.API.Core;

/// <summary>
/// An exception that indicates that the service couldn't complete
/// the request due to maybe some condition not being met.
///
/// By default we blame the client making the request for throwing this.
/// However in some cases this may not be the case.
/// </summary>
public class ServiceException : Exception
{
    /// <summary>
    /// The status code of this exception.
    /// </summary>
    public int StatusCode { get; set; } = 422;

    /// <summary>
    /// A more detailed explanation of the error, optional.
    /// </summary>
    public string? Detail { get; set; }

    public ServiceException() : base("Unprocessable Entity")
    {
    }

    public ServiceException(string message)
        : base(message)
    {
    }

    public ServiceException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public ServiceException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public ServiceException(int statusCode, string message, string? detail)
        : base(message)
    {
        StatusCode = statusCode;
        Detail = detail;
    }

    public ServiceException(int statusCode, string message, string? detail, Exception inner)
        : base(message, inner)
    {
        StatusCode = statusCode;
        Detail = detail;
    }
}
