using ARGarden.Backend.Errors;
using ARGarden.Backend.Models;
using Kontur.Results;

namespace ARGarden.Backend.Repositories;

public interface IModelsRepository
{
    Task<Result<ModelsRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(int skip = 0, int take = 30);

    Task<Result<ModelsRepositoryError, IEnumerable<int>>> GetModelVersionsAsync(Guid modelId);

    Task<Result<ModelsRepositoryError, byte[]>> GetModelImageAsync(Guid modelId, int version);

    Task<Result<ModelsRepositoryError, byte[]>> GetModelBundleAsync(Guid modelId, int version);
}