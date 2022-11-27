using ARGarden.Backend.Repositories;
using ARGarden.Backend.Settings;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace ARGarden.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton<IModelsRepository, FileSystemModelsRepository>()
            .AddSingleton<FileSystemRepositorySettingsProvider>()
            .AddSingleton<IConfigurationProvider, EnvironmentVariablesConfigurationProvider>();
}