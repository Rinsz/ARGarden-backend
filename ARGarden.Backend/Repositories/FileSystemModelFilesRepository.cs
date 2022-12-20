using Kontur.Results;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Repositories;

[PrimaryConstructor]
public partial class FileSystemModelFilesRepository : IModelFilesRepository
{
    private const string Unity3dExtension = ".unity3d";

    private readonly FileSystemRepositorySettingsProvider settingsProvider;
    private readonly ILogger<FileSystemModelFilesRepository> logger;

    public async Task<Result<ModelFilesRepositoryError, byte[]>> GetModelImageAsync(Guid modelId, int version) =>
        await this.GetFileBytesFromModelFolderAsync(
                modelId,
                version,
                "image.jpg",
                $"Image for model {modelId} is not found.")
            .ConfigureAwait(false);

    public async Task<Result<ModelFilesRepositoryError, byte[]>> GetModelBundleAsync(Guid modelId, int version) =>
        await this.GetFileBytesFromModelFolderAsync(
                modelId,
                version,
                $"{modelId}{Unity3dExtension}",
                $"Bundle for model {modelId} with version {version} is not found.")
            .ConfigureAwait(false);

    public Task<Result<ModelFilesRepositoryError>> DeleteModelFilesAsync(Guid modelId)
    {
        var modelRootDirectoryResult = this.GetStorageDirectory()
            .Then(sd => GetModelRootDirectory(sd, modelId));
        if (modelRootDirectoryResult.TryGetFault(out _, out var modelRootDirectory))
        {
            return Task.FromResult(Result<ModelFilesRepositoryError>.Succeed());
        }

        new DirectoryInfo(modelRootDirectory).Delete(true);
        return Task.FromResult(Result<ModelFilesRepositoryError>.Succeed());
    }

    public async Task<Result<ModelFilesRepositoryError>> CreateNewModelFilesAsync(
        Guid newModelId,
        Stream modelImageStream,
        Stream modelBundleStream)
    {
        var storageDirectoryResult = this.GetStorageDirectory();
        if (storageDirectoryResult.TryGetFault(out var fault, out var storageDirectory))
        {
            return fault;
        }

        var newModelDirectory = Directory.CreateDirectory(Path.Combine(storageDirectory, newModelId.ToString()));
        var newModelVersionDirectory = newModelDirectory.CreateSubdirectory("0");

        var versionPath = newModelVersionDirectory.FullName;
        var newImagePath = Path.Combine(versionPath, "image");
        await CreateNewFileAsync(newImagePath, modelImageStream).ConfigureAwait(false);

        var newBundlePath = Path.Combine(versionPath, $"{newModelId}{Unity3dExtension}");
        await CreateNewFileAsync(newBundlePath, modelBundleStream).ConfigureAwait(false);

        return Result.Succeed();
    }

    public async Task<Result<ModelFilesRepositoryError>> AddFilesForNewModelVersionAsync(
        Guid modelId,
        int modelVersion,
        Stream modelImageStream,
        Stream modelBundleStream)
    {
        var modelRootDirectoryResult = this.GetStorageDirectory()
            .Then(sd => GetModelRootDirectory(sd, modelId));
        if (modelRootDirectoryResult.TryGetFault(out var fault, out var modelRootDirectory))
        {
            return fault;
        }

        var newVersionPath = Directory.CreateDirectory(Path.Combine(modelRootDirectory, modelVersion.ToString())).FullName;
        var newImagePath = Path.Combine(newVersionPath, "image");
        await CreateNewFileAsync(newImagePath, modelImageStream).ConfigureAwait(false);

        var newBundlePath = Path.Combine(newVersionPath, $"{modelId}{Unity3dExtension}");
        await CreateNewFileAsync(newBundlePath, modelBundleStream).ConfigureAwait(false);

        return Result.Succeed();
    }

    private static async Task CreateNewFileAsync(string filePath, Stream fileStream)
    {
        var newFileStream = File.Create(filePath);
#pragma warning disable SA1312
        await using var _ = newFileStream.ConfigureAwait(false);
#pragma warning restore SA1312
        await fileStream.CopyToAsync(newFileStream).ConfigureAwait(false);
    }

    private static Result<ModelFilesRepositoryError, string> GetModelRootDirectory(string storageDirectory, Guid modelId)
    {
        var modelRootDirectory = Path.Combine(storageDirectory, modelId.ToString());
        return Directory.Exists(modelRootDirectory)
            ? modelRootDirectory
            : new ModelFilesRepositoryError(ApiErrorType.NotFound, $"Model with id: {modelId} does not exist.");
    }

    private static Result<ModelFilesRepositoryError, string> GetModelVersionDirectory(string modelRootDirectory, int version)
    {
        var modelVersionPath = Path.Combine(modelRootDirectory, version.ToString());
        return Directory.Exists(modelVersionPath)
            ? modelVersionPath
            : new ModelFilesRepositoryError(ApiErrorType.NotFound, $"No such version \"{version}\" of specified model");
    }

    private Result<ModelFilesRepositoryError, string> GetStorageDirectory()
    {
        var storageDirectory = this.settingsProvider.Get().StoragePath;
        return Directory.Exists(storageDirectory)
            ? storageDirectory
            : new ModelFilesRepositoryError(ApiErrorType.InternalServerError, "Storage does not exist.");
    }

    private async Task<Result<ModelFilesRepositoryError, byte[]>> GetFileBytesFromModelFolderAsync(
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
            : new ModelFilesRepositoryError(ApiErrorType.InternalServerError, fileNotFoundError);
    }
}