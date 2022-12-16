using MongoDB.Driver;
using ThreeXyNine.ARGarden.Api.Abstractions;

namespace ThreeXyNine.ARGarden.Api.Providers;

[PrimaryConstructor]
public partial class MongoClientSettingsProvider : IMongoClientSettingsProvider
{
    private readonly IPublicPropertiesProvider publicPropertiesProvider;
    private readonly ISecretPropertiesProvider secretPropertiesProvider;

    public MongoClientSettings GetSettings()
    {
        var (mongoDatabaseName, mongoLogin, mongoHosts) = this.publicPropertiesProvider.Get().MongoClientProperties;
        var secretProperties = this.secretPropertiesProvider.Get();

        return new()
        {
            Credential = MongoCredential.CreateCredential(mongoDatabaseName, mongoLogin, secretProperties.MongoPassword),
            Servers = mongoHosts.Select(MongoServerAddress.Parse),
        };
    }
}