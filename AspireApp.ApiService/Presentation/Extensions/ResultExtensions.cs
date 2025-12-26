using AspireApp.ApiService.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Extensions;

/// <summary>
/// Extension methods for converting Result types to HTTP responses.
/// This follows the pattern of mapping domain results to presentation layer responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an IResult HTTP response.
    /// </summary>
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        return result.Error.Type switch
        {
            ErrorType.Validation => Results.BadRequest(CreateErrorResponse(result.Error)),
            ErrorType.NotFound => Results.NotFound(CreateErrorResponse(result.Error)),
            ErrorType.Conflict => Results.Conflict(CreateErrorResponse(result.Error)),
            ErrorType.Unauthorized => Results.Json(
                CreateErrorResponse(result.Error),
                statusCode: StatusCodes.Status401Unauthorized),
            ErrorType.Forbidden => Results.Json(
                CreateErrorResponse(result.Error),
                statusCode: StatusCodes.Status403Forbidden),
            ErrorType.Failure => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: result.Error.Code),
            _ => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to an IResult HTTP response.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.Validation => Results.BadRequest(CreateErrorResponse(result.Error)),
            ErrorType.NotFound => Results.NotFound(CreateErrorResponse(result.Error)),
            ErrorType.Conflict => Results.Conflict(CreateErrorResponse(result.Error)),
            ErrorType.Unauthorized => Results.Json(
                CreateErrorResponse(result.Error),
                statusCode: StatusCodes.Status401Unauthorized),
            ErrorType.Forbidden => Results.Json(
                CreateErrorResponse(result.Error),
                statusCode: StatusCodes.Status403Forbidden),
            ErrorType.Failure => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: result.Error.Code),
            _ => Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to a Created HTTP response with location header.
    /// </summary>
    public static IResult ToHttpCreatedResult<T>(this Result<T> result, string location)
    {
        if (result.IsSuccess)
        {
            return Results.Created(location, result.Value);
        }

        return result.ToHttpResult();
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to a NoContent HTTP response.
    /// </summary>
    public static IResult ToHttpNoContentResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.ToHttpResult();
    }

    private static object CreateErrorResponse(Error error)
    {
        return new
        {
            error = new
            {
                code = error.Code,
                message = error.Message,
                type = error.Type.ToString()
            }
        };
    }
}

