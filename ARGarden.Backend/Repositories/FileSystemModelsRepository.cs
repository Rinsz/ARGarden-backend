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
            this.logger.LogError(e, "Failed to get model versions.");
            return new ModelsRepositoryError(ApiErrorType.InternalServerError, "Failed to get model versions.");
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
                $"{modelId}.unity3d",
                $"Bundle for model {modelId} with version {version} is not found.")
            .ConfigureAwait(false);

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