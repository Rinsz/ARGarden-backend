using MongoDB.Bson.Serialization.Attributes;

namespace ThreeXyNine.ARGarden.Api.Models;

[BsonIgnoreExtraElements]
internal record ModelMetaInternal(Guid ModelId, string Name, ModelGroup ModelGroup, int Version);