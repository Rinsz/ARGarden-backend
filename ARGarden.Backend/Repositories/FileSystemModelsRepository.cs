﻿using Kontur.Results;
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

    public async Task<Result<ModelsRepositoryError, IEnumerable<ModelMeta>>> GetMetasAsync(
        ModelGroup? modelGroup,
        string? modelName,
        int skip,
        int take) =>
        await this.modelMetasRepository.GetMetasAsync(modelGroup, modelName, skip, take)
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
        Guid modelId,
        string modelName,
        ModelGroup modelGroup,
        Stream modelImageStream,
        string imageExtension,
        Stream modelBundleStream)
    {
        var existenceResult = await this.modelMetasRepository.ModelExistAsync(modelId).ConfigureAwait(false);
        if (existenceResult.TryGetFault(out var fault, out var modelExist))
        {
            return ToModelsRepositoryError(fault);
        }

        return modelExist
            ? new ModelsRepositoryError(ApiErrorType.Conflict, $"Model with Id: {modelId} already exists.")
            : await this.modelFilesRepository.CreateNewModelFilesAsync(modelId, modelImageStream, imageExtension, modelBundleStream)
                .MapFault(ToModelsRepositoryError)
                .Then(async () =>
                    await this.modelMetasRepository.CreateModelMetaAsync(modelId, modelName, modelGroup, 0)
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