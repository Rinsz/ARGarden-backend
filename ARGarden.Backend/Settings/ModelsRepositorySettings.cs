namespace ThreeXyNine.ARGarden.Api.Settings;

public class ModelsRepositorySettings
{
    public string DatabaseName { get; init; } = default!;

    public string CollectionName { get; init; } = default!;

    public void Deconstruct(out string databaseName, out string collectionName)
    {
        databaseName = this.DatabaseName;
        collectionName = this.CollectionName;
    }
}