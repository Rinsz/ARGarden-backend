using MongoDB.Driver;
using ThreeXyNine.ARGarden.Api.Abstractions;

namespace ThreeXyNine.ARGarden.Api.Providers;

public class MongoClientProvider : IMongoClientProvider
{
    private readonly IMongoClientSettingsProvider clientSettingsProvider;
    private readonly Lazy<IMongoClient> clientCache;

    public MongoClientProvider(IMongoClientSettingsProvider clientSettingsProvider)
    {
        this.clientSettingsProvider = clientSettingsProvider;
        this.clientCache = new(this.CreateMongoClient);
    }

    public IMongoClient GetMongoClient() => this.clientCache.Value;

    private IMongoClient CreateMongoClient()
    {
        var clientSettings = this.clientSettingsProvider.GetSettings();
        return new MongoClient(clientSettings);
    }
}