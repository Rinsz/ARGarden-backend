using MongoDB.Driver;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IMongoCollectionProvider<TItem>
{
    IMongoCollection<TItem> GetCollection();
}