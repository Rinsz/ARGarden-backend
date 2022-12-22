using MongoDB.Driver;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Models;

namespace ThreeXyNine.ARGarden.Api.Providers;

[PrimaryConstructor]
public partial class ModelsRepositoryMongoCollectionProvider : IMongoCollectionProvider<ModelMetaInternal>
{
    private readonly IModelsRepositorySettingsProvider modelsRepositorySettingsProvider;
    private readonly IMongoDatabaseProvider databaseProvider;

    public IMongoCollection<ModelMetaInternal> GetCollection()
    {
        var (databaseName, collectionName) = this.modelsRepositorySettingsProvider.Get();
        return this.databaseProvider.GetDatabase(databaseName).GetCollection<ModelMetaInternal>(collectionName);
    }
}