namespace ThreeXyNine.ARGarden.Api.Settings;

public record MongoClientProperties
{
    public string MongoDatabaseName { get; init; } = default!;

    public string MongoLogin { get; init; } = default!;

    public string[] MongoHosts { get; init; } = default!;

    public void Deconstruct(out string mongoDatabaseName, out string mongoLogin, out string[] mongoHosts)
    {
        mongoDatabaseName = this.MongoDatabaseName;
        mongoLogin = this.MongoLogin;
        mongoHosts = this.MongoHosts;
    }
}