using MongoDB.Driver;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IMongoDatabaseProvider
{
    IMongoDatabase GetDatabase(string databaseName);
}