using MongoDB.Driver;
using ThreeXyNine.ARGarden.Api.Abstractions;

namespace ThreeXyNine.ARGarden.Api.Providers;

[PrimaryConstructor]
public partial class MongoDatabaseProvider : IMongoDatabaseProvider
{
    private readonly IMongoClientProvider mongoClientProvider;

    public IMongoDatabase GetDatabase(string databaseName) =>
        this.mongoClientProvider.GetMongoClient().GetDatabase(databaseName);
}