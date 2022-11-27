using Microsoft.Extensions.Configuration.EnvironmentVariables;
using ThreeXyNine.ARGarden.Api.Repositories;
using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton<IModelsRepository, FileSystemModelsRepository>()
            .AddSingleton<FileSystemRepositorySettingsProvider>()
            .AddSingleton<IConfigurationProvider, EnvironmentVariablesConfigurationProvider>();
}