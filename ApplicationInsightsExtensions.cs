using Microsoft.Extensions.DependencyInjection;

namespace codehacks_shared_package_lib
{
    public class ApplicationInsightsExtensions
    {
        public static void AddApplicationInsightsTelemetry(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
        }
    }
}