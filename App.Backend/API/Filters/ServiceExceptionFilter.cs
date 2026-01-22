// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Diagnostics;
using App.Backend.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace App.Backend.API.Filters;

// ============================================================================

/// <summary>
/// Exception filter for deep service related exceptions.
/// For example, a service way down in the call stack might need to propegate
/// some form of HTTP Error to the client because further processing isn't
/// possible. Or for example we tried to find something and it isn't there.
///
/// Rather than 'handle' it, we just throw an exception and respond back.
/// </summary>
public class ServiceExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ServiceException serviceException)
            return;

        context.Result = serviceException.StatusCode switch
        {
            StatusCodes.Status403Forbidden => new ForbidResult(),
            StatusCodes.Status404NotFound => new NotFoundResult(),
            _ => new ObjectResult(new ProblemDetails
            {
                Type = $"https://http.cat/{serviceException.StatusCode}",
                Title = serviceException.Message,
                Detail = serviceException.Detail,
                Status = serviceException.StatusCode,
                Instance = context.HttpContext.Request.Path,
                Extensions = new Dictionary<string, object?>
                {
                    ["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
                }
            })
            {
                ContentTypes = { "application/problem+json" }
            }
        };

        context.ExceptionHandled = true;
    }
}
