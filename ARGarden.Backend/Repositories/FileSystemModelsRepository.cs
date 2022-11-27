using Kontur.Results;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;
using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Repositories;

[PrimaryConstructor]
public partial class FileSystemModelsRepository : IModelsRepository
{
    private readonly FileSystemRepositorySettingsProvider settingsProvider;
    private readonly ILogger<FileSystemModelsRepository> logger;

    public Task<Result<ModelsRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(int skip = 0, int take = 30)
    {
        var metasStub = new[]
            {
                new Guid("e7338f32-37ca-483c-9cf4-840253d75f72"),
                new Guid("6b6b6010-9fdb-40ac-905d-998732b30f20"),
                new Guid("70d008dd-0fcc-4985-806e-cfafd86d0476"),
            }
            .Skip(skip)
            .Take(take)
            .Select((id, num) => new ModelMeta(
                id,
                $"Server model {num}",
                (ModelGroup)Math.Abs(id.GetHashCode() % (Enum.GetNames<ModelGroup>().Length - 1)),
                0));

        return Task.FromResult(Result<ModelsRepositoryError, IEnumerable<ModelMeta>>.Succeed(metasStub));
    }

    public Task<Result<ModelsRepositoryError, IEnumerable<int>>> GetModelVersionsAsync(Guid modelId)
    {
        var modelRootDirectoryResult = this.GetStorageDirectory()
            .Then(storageDir => GetModelRootDirectory(storageDir, modelId))
            .OnFailure(error => this.logger.LogError(error.ToString()));

        if (modelRootDirectoryResult.TryGetFault(out var fault, out var modelRootDirectory))
        {
            return Task.FromResult(Result<ModelsRepositoryError, IEnumerable<int>>.Fail(fault));
        }

        var versions = new DirectoryInfo(modelRootDirectory).EnumerateDirectories().Select(x => int.Parse(x.Name));
        return Task.FromResult(Result<ModelsRepositoryError, IEnumerable<int>>.Succeed(versions));
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