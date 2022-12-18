using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Providers;

[PrimaryConstructor]
public partial class ModelsRepositorySettingsProvider : IModelsRepositorySettingsProvider
{
    private readonly IPublicPropertiesProvider publicPropertiesProvider;

    public ModelsRepositorySettings Get()
    {
        var (databaseName, collectionName) = this.publicPropertiesProvider.Get().ModelsRepositorySettings;
        return new()
        {
            DatabaseName = databaseName,
            CollectionName = collectionName,
        };
    }
}