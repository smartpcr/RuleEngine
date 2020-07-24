namespace Common.Config
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class OptionsBuilder
    {
        public static IServiceCollection ConfigureSettings<T>(this IServiceCollection services) where T : class, new()
        {
            services.AddOptions<T>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(typeof(T).Name).Bind(settings);
                });
            return services;
        }

        public static T GetConfiguredSettings<T>(this IConfiguration configuration, string sectionName = null)
            where T : class, new()
        {
            var settings = new T();
            sectionName = sectionName ?? typeof(T).Name;
            configuration.Bind(typeof(T).Name, settings);
            return settings;
        }
    }
}