using Microsoft.AspNetCore.Mvc;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;
using static ThreeXyNine.ARGarden.Api.Constants.ContentTypes;

namespace ThreeXyNine.ARGarden.Api.Controllers;

[ApiController]
[Route("api/models")]
public class ModelsController : ControllerBase
{
    private readonly IModelsRepository modelsRepository;
    private readonly ILogger<ModelsController> logger;

    public ModelsController(IModelsRepository modelsRepository, ILogger<ModelsController> logger)
    {
        this.modelsRepository = modelsRepository;
        this.logger = logger;
    }

    [HttpGet("metas")]
    public async Task<ActionResult<IEnumerable<ModelMeta>>> GetModelMetas(int skip = 0, int take = 100)
    {
        this.logger.LogInformation("Getting available model metas.");
        var getMetasResult = await this.modelsRepository.GetMetasAsync(skip, take).ConfigureAwait(false);

        return getMetasResult.TryGetValue(out var metas, out var fault)
            ? this.Ok(metas)
            : this.ConvertFaultToActionResult(fault);
    }

    [HttpGet("versions/{modelId:guid}")]
    public async Task<ActionResult<IEnumerable<int>>> GetModelVersions(Guid modelId)
    {
        this.logger.LogInformation($"Getting model versions. ModelId: {modelId}");
        var versionsResult = await this.modelsRepository.GetModelVersionsAsync(modelId).ConfigureAwait(false);

        return versionsResult.TryGetValue(out var versions, out var fault)
            ? this.Ok(versions)
            : this.ConvertFaultToActionResult(fault);
    }

    [HttpGet("images/{modelId:guid}/{version:int}")]
    public async Task<IActionResult> GetModelImageAsync(Guid modelId, int version)
    {
        this.logger.LogInformation($"Getting model image. ModelId: {modelId}; Version: {version}");
        var fileBytesResult = await this.modelsRepository.GetModelImageAsync(modelId, version).ConfigureAwait(false);

        return fileBytesResult.TryGetValue(out var fileBytes, out var fault)
            ? this.File(fileBytes, JpegContentType)
            : this.ConvertFaultToActionResult(fault);
    }

    [HttpGet("bundles/{modelId:guid}/{version:int}")]
    public async Task<IActionResult> GetModelBundleAsync(Guid modelId, int version)
    {
        this.logger.LogInformation($"Getting asset bundle for model. ModelId: {modelId}; Version: {version}");
        var bundleBytesResult = await this.modelsRepository.GetModelBundleAsync(modelId, version).ConfigureAwait(false);

        return bundleBytesResult.TryGetValue(out var bundleBytes, out var fault)
            ? this.File(bundleBytes, AssetBundleContentType)
            : this.ConvertFaultToActionResult(fault);
    }

    private ActionResult ConvertFaultToActionResult(ModelsRepositoryError error)
    {
        var (apiErrorType, description) = error;
        return apiErrorType switch
        {
            ApiErrorType.NotFound => this.NotFound(description),
            ApiErrorType.InternalServerError => this.StatusCode(StatusCodes.Status500InternalServerError),
#pragma warning disable S3928
            _ => throw new ArgumentOutOfRangeException(nameof(apiErrorType), apiErrorType, "Unprocessable error type received."),
#pragma warning restore S3928
        };
    }
}