using maltedmoniker.pipeline.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace maltedmoniker.pipeline
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPiplineBuilders(this IServiceCollection services)
        {
            return services.AddTransient<IPipelineBuilderFactory, PipelineBuilderFactory>();
        }
    }
}
