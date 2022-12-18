namespace ThreeXyNine.ARGarden.Api.Settings;

public record PublicProperties
{
    public MongoClientProperties MongoClientProperties { get; init; } = default!;

    public ModelsRepositorySettings ModelsRepositorySettings { get; init; } = default!;
}