using Kontur.Results;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IModelsRepository
{
    Task<Result<ModelsRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(int skip = 0, int take = 30);

    Task<Result<ModelsRepositoryError, IEnumerable<int>>> GetModelVersionsAsync(Guid modelId);

    Task<Result<ModelsRepositoryError, byte[]>> GetModelImageAsync(Guid modelId, int version);

    Task<Result<ModelsRepositoryError, byte[]>> GetModelBundleAsync(Guid modelId, int version);
}