namespace ThreeXyNine.ARGarden.Api.Settings;

public class PublicProperties
{
    public MongoClientProperties MongoClientProperties { get; init; } = default!;

    public ModelsRepositorySettings ModelsRepositorySettings { get; init; } = default!;
}