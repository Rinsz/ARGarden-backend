using System.Text.RegularExpressions;
using Kontur.Results;
using MongoDB.Driver;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;

namespace ThreeXyNine.ARGarden.Api.Repositories;

[PrimaryConstructor]
public partial class MongoModelMetasRepository : IModelMetasRepository
{
    private readonly IMongoCollectionProvider<ModelMetaInternal> mongoCollectionProvider;
    private readonly ILogger<MongoModelMetasRepository> logger;

    public async Task<Result<ModelMetasRepositoryError, bool>> ModelExistAsync(Guid modelId)
    {
        try
        {
            var collection = this.mongoCollectionProvider.GetCollection();
            var metas = await collection.Find(meta => meta.ModelId == modelId).ToListAsync().ConfigureAwait(false);
            return metas.Any();
        }
        catch (Exception e)
        {
            this.logger.LogError(e, $"Failed to check model existence. ModelId: {modelId}");
            return new ModelMetasRepositoryError(ApiErrorType.InternalServerError, "Failed to check model existence.");
        }
    }

    public async Task<Result<ModelMetasRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(
        ModelGroup? modelGroup,
        string? modelName,
        int skip,
        int take)
    {
        try
        {
            var collection = this.mongoCollectionProvider.GetCollection();
            var filter = BuildFilter(modelGroup, modelName);

            var metas = await collection
                .Find(filter)
                .SortBy(meta => meta.Name)
                .Skip(skip)
                .Limit(take)
                .ToListAsync()
                .ConfigureAwait(false);

            return metas.Select(ToModelMeta).ToList();
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to get model metas.");
            return new ModelMetasRepositoryError(ApiErrorType.InternalServerError, "Failed to get model metas.");
        }
    }

    public async Task<Result<ModelMetasRepositoryError, IEnumerable<int>>> GetModelVersionsAsync(Guid modelId)
    {
        try
        {
            var collection = this.mongoCollectionProvider.GetCollection();
            var metas = await collection
                .Find(meta => meta.ModelId == modelId)
                .ToListAsync()
                .ConfigureAwait(false);

            return metas.Any()
                ? metas.Select(meta => meta.Version).ToArray()
                : new ModelMetasRepositoryError(ApiErrorType.NotFound, $"Model with id {modelId} is not found.");
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to get model versions.");
            return new ModelMetasRepositoryError(ApiErrorType.InternalServerError, "Failed to get model versions.");
        }
    }

    public async Task<Result<ModelMetasRepositoryError, ModelMeta>> CreateModelMetaAsync(
        Guid modelId,
        string modelName,
        ModelGroup modelGroup,
        int version)
    {
        try
        {
            var metasCollection = this.mongoCollectionProvider.GetCollection();
            var meta = new ModelMetaInternal(modelId, modelName, modelGroup, version);
            await metasCollection.InsertOneAsync(meta).ConfigureAwait(false);
            return ToModelMeta(meta);
        }
        catch (Exception e)
        {
            const string errorString = "Failed to create model meta";
            this.logger.LogError(e, errorString);
            return new ModelMetasRepositoryError(ApiErrorType.InternalServerError, errorString);
        }
    }

    public async Task<Result<ModelMetasRepositoryError>> DeleteModelMetasAsync(Guid modelId)
    {
        try
        {
            var collection = this.mongoCollectionProvider.GetCollection();
            var filter = Builders<ModelMetaInternal>.Filter.Where(meta => meta.ModelId == modelId);
            await collection.DeleteManyAsync(filter).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, $"Failed to delete model. ModelId: {modelId}");
            return new ModelMetasRepositoryError(ApiErrorType.InternalServerError, "Failed to delete model");
        }

        return Result.Succeed();
    }

    private static FilterDefinition<ModelMetaInternal> BuildFilter(ModelGroup? modelGroup, string? modelName)
    {
        var filters = new List<FilterDefinition<ModelMetaInternal>>();
        if (modelGroup is { } group)
            filters.Add(Builders<ModelMetaInternal>.Filter.Eq(meta => meta.ModelGroup, group));
        if (!string.IsNullOrWhiteSpace(modelName))
        {
            var regex = new Regex(modelName, RegexOptions.IgnoreCase);
            filters.Add(Builders<ModelMetaInternal>.Filter.Regex(meta => meta.Name, regex));
        }

        var filter = filters.Any()
            ? Builders<ModelMetaInternal>.Filter.And(filters)
            : Builders<ModelMetaInternal>.Filter.Empty;
        return filter;
    }

    private static ModelMeta ToModelMeta(ModelMetaInternal internalMeta) =>
        new(internalMeta.ModelId, internalMeta.Name, internalMeta.ModelGroup, internalMeta.Version);
}