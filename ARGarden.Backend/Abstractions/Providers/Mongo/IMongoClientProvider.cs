using MongoDB.Driver;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IMongoClientProvider
{
    IMongoClient GetMongoClient();
}