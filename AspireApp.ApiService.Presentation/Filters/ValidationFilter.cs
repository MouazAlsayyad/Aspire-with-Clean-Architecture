using FluentValidation;

namespace AspireApp.ApiService.Presentation.Filters;

/// <summary>
/// Endpoint filter that automatically validates request DTOs using FluentValidation.
/// </summary>
public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Find all validators from the service provider
        var validatorType = typeof(IValidator<>);
        var serviceProvider = context.HttpContext.RequestServices;

        // Check each argument for validation
        foreach (var argument in context.Arguments)
        {
            if (argument == null)
                continue;

            var argumentType = argument.GetType();
            var genericValidatorType = validatorType.MakeGenericType(argumentType);

            // Try to get validator from DI
            var validator = serviceProvider.GetService(genericValidatorType) as IValidator;

            if (validator != null)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    // Group errors by property name for structured access
                    var errorsByProperty = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );

                    // Create a flat list of all errors for easy iteration
                    var allErrors = validationResult.Errors
                        .Select(e => new
                        {
                            property = e.PropertyName,
                            message = e.ErrorMessage,
                            attemptedValue = e.AttemptedValue?.ToString()
                        })
                        .ToArray();

                    return Results.BadRequest(new
                    {
                        error = new
                        {
                            code = "Validation",
                            message = "One or more validation errors occurred.",
                            errorsByProperty,
                            allErrors
                        }
                    });
                }
            }
        }

        return await next(context);
    }
}

