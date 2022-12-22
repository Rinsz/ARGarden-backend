using MongoDB.Bson.Serialization.Attributes;

namespace ThreeXyNine.ARGarden.Api.Models;

[BsonIgnoreExtraElements]
public record ModelMetaInternal(Guid ModelId, string Name, ModelGroup ModelGroup, int Version);