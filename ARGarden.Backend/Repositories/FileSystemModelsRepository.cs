using Kontur.Results;
using MongoDB.Driver;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;
using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Repositories;

[PrimaryConstructor]
public partial class FileSystemModelsRepository : IModelsRepository
{
    private const string Unity3dExtension = ".unity3d";

    private readonly FileSystemRepositorySettingsProvider settingsProvider;
    private readonly IMongoCollectionProvider<ModelMeta> mongoCollectionProvider;
    private readonly ILogger<FileSystemModelsRepository> logger;

    public async Task<Result<ModelsRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(int skip = 0, int take = 30)
    {
        try
        {
            var collection = this.mongoCollectionProvider.GetCollection();
            var metas = await collection
                .Find(_ => true)
                .Skip(skip)
                .Limit(take)
                .ToListAsync()
                .ConfigureAwait(false);

            return metas;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to get model metas.");
            return new ModelsRepositoryError(ApiErrorType.InternalServerError, "Failed to get model metas.");
        }
    }

    public async Task<Result<ModelsRepositoryError, IEnumerable<int>>> GetModelVersionsAsync(Guid modelId)
    {
        try
        {
            var collection = this.mongoCollectionProvider.GetCollection();
            var metas = await collection
                .Find(meta => meta.Id == modelId)
                .ToListAsync()
                .ConfigureAwait(false);

            return metas.Select(meta => meta.Version).ToArray();
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to get model versions.");
            return new ModelsRepositoryError(ApiErrorType.InternalServerError, "Failed to get model versions.");
        }
    }

    public async Task<Result<ModelsRepositoryError, byte[]>> GetModelImageAsync(Guid modelId, int version) =>
        await this.GetFileBytesFromModelFolderAsync(
                modelId,
                version,
                "image.jpg",
                $"Image for model {modelId} is not found.")
            .ConfigureAwait(false);

    public async Task<Result<ModelsRepositoryError, byte[]>> GetModelBundleAsync(Guid modelId, int version) =>
        await this.GetFileBytesFromModelFolderAsync(
                modelId,
                version,
                $"{modelId}{Unity3dExtension}",
                $"Bundle for model {modelId} with version {version} is not found.")
            .ConfigureAwait(false);

    public async Task<Result<ModelsRepositoryError>> RemoveModelAsync(Guid modelId)
    {
        try
        {
            var collection = this.mongoCollectionProvider.GetCollection();
            var filter = Builders<ModelMeta>.Filter.Where(meta => meta.Id == modelId);
            await collection.DeleteManyAsync(filter).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, $"Failed to delete model. ModelId: {modelId}");
            return new ModelsRepositoryError(ApiErrorType.InternalServerError, "Failed to delete model");
        }

        var modelRootDirectoryResult = this.GetStorageDirectory()
            .Then(sd => GetModelRootDirectory(sd, modelId));
        if (modelRootDirectoryResult.TryGetFault(out _, out var modelRootDirectory))
        {
            return Result.Succeed();
        }

        new DirectoryInfo(modelRootDirectory).Delete(true);
        return Result.Succeed();
    }

    public async Task<Result<ModelsRepositoryError, ModelMeta>> CreateModelAsync(string modelName, ModelGroup modelGroup, Stream modelImageStream, Stream modelBundleStream)
    {
        var storageDirectoryResult = this.GetStorageDirectory();
        if (storageDirectoryResult.TryGetFault(out var fault, out var storageDirectory))
        {
            return fault;
        }

        var newModelId = Guid.NewGuid();
        var newModelDirectory = Directory.CreateDirectory(Path.Combine(storageDirectory, newModelId.ToString()));
        var newModelVersionDirectory = newModelDirectory.CreateSubdirectory("0");
        var versionPath = newModelVersionDirectory.FullName;

        var imageFs = File.Create(Path.Combine(versionPath, "image"));
#pragma warning disable SA1312
        await using var _ = imageFs.ConfigureAwait(false);
#pragma warning restore SA1312
        await modelImageStream.CopyToAsync(imageFs).ConfigureAwait(false);

        var bundleFs = File.Create(Path.Combine(versionPath, $"{newModelId}{Unity3dExtension}"));
#pragma warning disable SA1312
        await using var __ = bundleFs.ConfigureAwait(false);
#pragma warning restore SA1312
        await modelBundleStream.CopyToAsync(bundleFs).ConfigureAwait(false);

        var metasCollection = this.mongoCollectionProvider.GetCollection();
        var meta = new ModelMeta(newModelId, modelName, modelGroup, 0);
        await metasCollection.InsertOneAsync(meta).ConfigureAwait(false);
        return meta;
    }

    private static Result<ModelsRepositoryError, string> GetModelRootDirectory(string storageDirectory, Guid modelId)
    {
        var modelRootDirectory = Path.Combine(storageDirectory, modelId.ToString());
        return Directory.Exists(modelRootDirectory)
            ? modelRootDirectory
            : new ModelsRepositoryError(ApiErrorType.NotFound, $"Model with id: {modelId} does not exist.");
    }

    private static Result<ModelsRepositoryError, string> GetModelVersionDirectory(string modelRootDirectory, int version)
    {
        var modelVersionPath = Path.Combine(modelRootDirectory, version.ToString());
        return Directory.Exists(modelVersionPath)
            ? modelVersionPath
            : new ModelsRepositoryError(ApiErrorType.NotFound, $"No such version \"{version}\" of specified model");
    }

    private Result<ModelsRepositoryError, string> GetStorageDirectory()
    {
        var storageDirectory = this.settingsProvider.Get().StoragePath;
        return Directory.Exists(storageDirectory)
            ? storageDirectory
            : new ModelsRepositoryError(ApiErrorType.InternalServerError, "Storage does not exist.");
    }

    private async Task<Result<ModelsRepositoryError, byte[]>> GetFileBytesFromModelFolderAsync(
        Guid modelId,
        int version,
        string fileName,
        string fileNotFoundError)
    {
        var getDirectoryResult = this.GetStorageDirectory()
            .Then(storageDir => GetModelRootDirectory(storageDir, modelId))
            .Then(modelRootDir => GetModelVersionDirectory(modelRootDir, version))
            .OnFailure(error => this.logger.LogError(error.ToString()));

        if (getDirectoryResult.TryGetFault(out var fault, out var modelVersionDir))
            return fault;

        var filePath = Path.Combine(modelVersionDir, fileName);
        return File.Exists(filePath)
            ? await File.ReadAllBytesAsync(filePath).ConfigureAwait(false)
            : new ModelsRepositoryError(ApiErrorType.InternalServerError, fileNotFoundError);
    }
}