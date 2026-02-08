// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core;

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
    /// Gets or sets the status code associated with this exception. Defaults to 422.
    /// </summary>
    public int StatusCode { get; set; } = 422;

    /// <summary>
    /// Gets or sets an optional, more detailed description of the error.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class.
    /// </summary>
    public ServiceException() : base()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified error message.
    /// </summary>
    public ServiceException(string message) : base(message)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified status code and error message.
    /// </summary>
    /// <param name="statusCode">The status code associated with the error.</param>
    /// <param name="message">The message that describes the error.</param>
    public ServiceException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a status code, message, and detailed description.
    /// </summary>
    /// <param name="statusCode">The status code associated with the error.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="detail">Additional detailed information about the error.</param>
    public ServiceException(int statusCode, string message, string detail) : base(message)
    {
        StatusCode = statusCode;
        Detail = detail;
    }

    /// <summary>
    /// Throws a <see cref="ServiceException"/> with the specified message if the provided condition is <c>true</c>.
    /// </summary>
    /// <param name="argument">The condition to evaluate; if <c>true</c>, the exception is thrown.</param>
    /// <param name="message">The message for the thrown exception.</param>
    /// <exception cref="ServiceException">Thrown when <paramref name="argument"/> is <c>true</c>.</exception>
    public static void ThrowIf(bool argument, string message)
    {
        if (argument) throw new ServiceException(message);
    }

    /// <summary>
    /// Throws a <see cref="ServiceException"/> with the specified status code and message if the provided condition is <c>true</c>.
    /// </summary>
    /// <param name="argument">The condition to evaluate; if <c>true</c>, the exception is thrown.</param>
    /// <param name="statusCode">The status code to associate with the thrown exception.</param>
    /// <param name="message">The message for the thrown exception.</param>
    /// <exception cref="ServiceException">Thrown when <paramref name="argument"/> is <c>true</c>.</exception>
    public static void ThrowIf(bool argument, int statusCode, string message)
    {
        if (argument) throw new ServiceException(statusCode, message);
    }
}
