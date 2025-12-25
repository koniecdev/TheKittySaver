using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.API.ExceptionHandlers;
using TheKittySaver.AdoptionSystem.API.Pipeline;

namespace TheKittySaver.AdoptionSystem.API;

internal static class ApiDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApi()
        {
            services.AddExceptionHandler<ArgumentExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.PipelineBehaviors = [typeof(FailureLoggingBehaviour<,>)];
            });

            return services;
        }
    }
}
