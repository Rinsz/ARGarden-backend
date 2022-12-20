using Kontur.Results;
using ThreeXyNine.ARGarden.Api.Errors;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IModelFilesRepository
{
    Task<Result<ModelFilesRepositoryError, byte[]>> GetModelImageAsync(Guid modelId, int version);

    Task<Result<ModelFilesRepositoryError, byte[]>> GetModelBundleAsync(Guid modelId, int version);

    Task<Result<ModelFilesRepositoryError>> DeleteModelFilesAsync(Guid modelId);

    Task<Result<ModelFilesRepositoryError>> CreateNewModelFilesAsync(
        Guid newModelId,
        Stream modelImageStream,
        string imageExtension,
        Stream modelBundleStream);

    Task<Result<ModelFilesRepositoryError>> AddFilesForNewModelVersionAsync(
        Guid modelId,
        int modelVersion,
        Stream modelImageStream,
        string imageExtension,
        Stream modelBundleStream);
}