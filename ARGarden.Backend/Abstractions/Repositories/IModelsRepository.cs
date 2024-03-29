﻿using Kontur.Results;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IModelsRepository
{
    Task<Result<ModelsRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(
        ModelGroup? modelGroup,
        string? modelName,
        int skip,
        int take);

    Task<Result<ModelsRepositoryError, IEnumerable<int>>> GetModelVersionsAsync(Guid modelId);

    Task<Result<ModelsRepositoryError, byte[]>> GetModelImageAsync(Guid modelId, int version);

    Task<Result<ModelsRepositoryError, byte[]>> GetModelBundleAsync(Guid modelId, int version);

    Task<Result<ModelsRepositoryError>> RemoveModelAsync(Guid modelId);

    Task<Result<ModelsRepositoryError, ModelMeta>> CreateModelAsync(
        Guid modelId,
        string modelName,
        ModelGroup modelGroup,
        Stream modelImageStream,
        string imageExtension,
        Stream modelBundleStream);

    Task<Result<ModelsRepositoryError>> UpdateModelAsync(
        Guid modelId,
        string modelName,
        ModelGroup modelGroup,
        Stream modelImageStream,
        string imageExtension,
        Stream modelBundleStream);
}