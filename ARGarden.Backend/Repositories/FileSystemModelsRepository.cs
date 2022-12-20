using Kontur.Results;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;

namespace ThreeXyNine.ARGarden.Api.Repositories;

[PrimaryConstructor]
public partial class FileSystemModelsRepository : IModelsRepository
{
    private readonly IModelMetasRepository modelMetasRepository;
    private readonly IModelFilesRepository modelFilesRepository;
    private readonly ILogger<FileSystemModelsRepository> logger;

    public async Task<Result<ModelsRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(int skip = 0, int take = 30) =>
        await this.modelMetasRepository.GetMetasAsync(skip, take)
            .MapFault(ToModelsRepositoryError)
            .ConfigureAwait(false);

    public async Task<Result<ModelsRepositoryError, IEnumerable<int>>> GetModelVersionsAsync(Guid modelId) =>
        await this.modelMetasRepository.GetModelVersionsAsync(modelId)
            .MapFault(ToModelsRepositoryError)
            .ConfigureAwait(false);

    public async Task<Result<ModelsRepositoryError, byte[]>> GetModelImageAsync(Guid modelId, int version) =>
        await this.modelFilesRepository.GetModelImageAsync(modelId, version)
            .MapFault(ToModelsRepositoryError)
            .ConfigureAwait(false);

    public async Task<Result<ModelsRepositoryError, byte[]>> GetModelBundleAsync(Guid modelId, int version) =>
        await this.modelFilesRepository.GetModelBundleAsync(modelId, version)
            .MapFault(ToModelsRepositoryError)
            .ConfigureAwait(false);

    public async Task<Result<ModelsRepositoryError>> RemoveModelAsync(Guid modelId) =>
        await this.modelMetasRepository.DeleteModelMetasAsync(modelId)
            .MapFault(ToModelsRepositoryError)
            .Then(async () =>
                await this.modelFilesRepository.DeleteModelFilesAsync(modelId)
                    .MapFault(ToModelsRepositoryError)
                    .ConfigureAwait(false))
            .ConfigureAwait(false);

    public async Task<Result<ModelsRepositoryError, ModelMeta>> CreateModelAsync(
        string modelName,
        ModelGroup modelGroup,
        Stream modelImageStream,
        string imageExtension,
        Stream modelBundleStream)
    {
        var newModelId = Guid.NewGuid();
        return await this.modelFilesRepository.CreateNewModelFilesAsync(newModelId, modelImageStream, imageExtension, modelBundleStream)
            .MapFault(ToModelsRepositoryError)
            .Then(async () =>
                await this.modelMetasRepository.CreateModelMetaAsync(newModelId, modelName, modelGroup, 0)
                    .MapFault(ToModelsRepositoryError)
                    .ConfigureAwait(false))
            .ConfigureAwait(false);
    }

    public async Task<Result<ModelsRepositoryError>> UpdateModelAsync(
        Guid modelId,
        string modelName,
        ModelGroup modelGroup,
        Stream modelImageStream,
        string imageExtension,
        Stream modelBundleStream)
    {
        var getVersionsResult = await this.modelMetasRepository.GetModelVersionsAsync(modelId)
            .MapFault(ToModelsRepositoryError)
            .ConfigureAwait(false);

        if (getVersionsResult.TryGetFault(out var fault, out var versions))
        {
            return fault;
        }

        var modelVersions = versions.ToArray();
        if (!modelVersions.Any())
        {
            var errorString = $"Model with id {modelId} was not found";
            this.logger.LogError(errorString);
            return new ModelsRepositoryError(ApiErrorType.NotFound, errorString);
        }

        var newVersion = modelVersions.Max() + 1;
        return await this.modelFilesRepository.AddFilesForNewModelVersionAsync(
                modelId,
                newVersion,
                modelImageStream,
                imageExtension,
                modelBundleStream)
            .MapFault(ToModelsRepositoryError)
            .Then(async () =>
                await this.modelMetasRepository.CreateModelMetaAsync(modelId, modelName, modelGroup, newVersion)
                    .MapFault(ToModelsRepositoryError)
                    .ConfigureAwait(false))
            .ConfigureAwait(false);
    }

    private static ModelsRepositoryError ToModelsRepositoryError(ModelFilesRepositoryError error) =>
        new(error.ErrorType, error.Description);

    private static ModelsRepositoryError ToModelsRepositoryError(ModelMetasRepositoryError error) =>
        new(error.ErrorType, error.Description);
}