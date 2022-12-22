using Kontur.Results;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IModelMetasRepository
{
    Task<Result<ModelMetasRepositoryError, bool>> ModelExistAsync(Guid modelId);

    Task<Result<ModelMetasRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(int skip = 0, int take = 30);

    Task<Result<ModelMetasRepositoryError, IEnumerable<int>>> GetModelVersionsAsync(Guid modelId);

    Task<Result<ModelMetasRepositoryError, ModelMeta>> CreateModelMetaAsync(Guid modelId, string modelName, ModelGroup modelGroup, int version);

    Task<Result<ModelMetasRepositoryError>> DeleteModelMetasAsync(Guid modelId);
}