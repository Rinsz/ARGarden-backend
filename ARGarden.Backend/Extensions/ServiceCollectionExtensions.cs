using Microsoft.Extensions.Configuration.EnvironmentVariables;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Models;
using ThreeXyNine.ARGarden.Api.Providers;
using ThreeXyNine.ARGarden.Api.Repositories;
using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton<IPublicPropertiesProvider, PublicPropertiesProvider>()
            .AddSingleton<ISecretPropertiesProvider, SecretPropertiesProvider>()
            .AddSingleton<IMongoClientSettingsProvider, MongoClientSettingsProvider>()

            .AddSingleton<IMongoClientProvider, MongoClientProvider>()
            .AddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>()
            .AddSingleton<IMongoCollectionProvider<ModelMetaInternal>, ModelsRepositoryMongoCollectionProvider>()

            .AddSingleton<IModelsRepositorySettingsProvider, ModelsRepositorySettingsProvider>()
            .AddSingleton<IModelsRepository, FileSystemModelsRepository>()
            .AddSingleton<IModelMetasRepository, MongoModelMetasRepository>()
            .AddSingleton<IModelFilesRepository, FileSystemModelFilesRepository>()
            .AddSingleton<FileSystemRepositorySettingsProvider>()
            .AddSingleton<IConfigurationProvider, EnvironmentVariablesConfigurationProvider>();
}