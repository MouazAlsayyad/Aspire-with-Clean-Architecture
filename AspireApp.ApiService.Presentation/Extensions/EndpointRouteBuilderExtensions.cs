using System.Reflection;

namespace AspireApp.ApiService.Presentation.Extensions;

/// <summary>
/// Extension methods for automatically registering endpoints
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Automatically discovers and registers all endpoint mapping methods.
    /// Finds all static classes with methods named "Map*Endpoints" that accept IEndpointRouteBuilder
    /// and invokes them to register endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var presentationAssembly = Assembly.GetAssembly(typeof(EndpointRouteBuilderExtensions));

        if (presentationAssembly == null)
            return app;

        // Find all static classes in the Endpoints or Notifications namespace
        var endpointClasses = presentationAssembly
            .GetTypes()
            .Where(t => t.IsClass &&
                       t.IsAbstract &&
                       t.IsSealed && // static classes are abstract and sealed
                       (t.Namespace?.Contains("Endpoints") == true || t.Namespace?.Contains("Notifications") == true))
            .ToList();

        // Find all methods named "Map*Endpoints" that accept IEndpointRouteBuilder
        foreach (var endpointClass in endpointClasses)
        {
            var mapMethods = endpointClass
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name.StartsWith("Map") &&
                           m.Name.EndsWith("Endpoints") &&
                           m.GetParameters().Length == 1 &&
                           m.GetParameters()[0].ParameterType == typeof(IEndpointRouteBuilder))
                .ToList();

            // Invoke each mapping method
            foreach (var method in mapMethods)
            {
                try
                {
                    method.Invoke(null, [app]);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to register endpoints from {endpointClass.Name}.{method.Name}", ex);
                }
            }
        }

        return app;
    }
}

