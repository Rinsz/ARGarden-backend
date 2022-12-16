using MongoDB.Driver;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IMongoClientSettingsProvider
{
    MongoClientSettings GetSettings();
}