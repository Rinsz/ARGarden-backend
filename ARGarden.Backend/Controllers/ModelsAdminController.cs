using Microsoft.AspNetCore.Mvc;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Extensions;
using ThreeXyNine.ARGarden.Api.Models;
using static ThreeXyNine.ARGarden.Api.Constants.ContentTypes;
using static ThreeXyNine.ARGarden.Api.Constants.FileExtensions;

namespace ThreeXyNine.ARGarden.Api.Controllers;

[ApiController]
[Route("api/admin/models")]
public class ModelsAdminController : ControllerBase
{
    private static readonly IReadOnlySet<string> SupportedAssetBundleContentTypes = new HashSet<string>
    {
        AssetBundleContentType,
        OctetStreamContentType,
    };

    private static readonly IReadOnlySet<string> SupportedImageContentTypes = new HashSet<string>
    {
        JpegContentType,
        JpgContentType,
        PngContentType,
    };

    private static readonly IReadOnlySet<string> SupportedImageExtensions = new HashSet<string>
    {
        JpegExtension,
        JpgExtension,
        PngExtension,
    };

    private readonly IModelsRepository modelsRepository;
    private readonly ILogger<ModelsAdminController> logger;

    public ModelsAdminController(IModelsRepository modelsRepository, ILogger<ModelsAdminController> logger)
    {
        this.modelsRepository = modelsRepository;
        this.logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateModelAsync([FromForm] CreateModelRequest request)
    {
        var (modelName, modelGroup, modelImageFile, modelBundleFile) = request;
        if (!SupportedImageContentTypes.Contains(modelImageFile.ContentType))
            return this.ValidationProblem("Unsupported image type received");

        if (!SupportedAssetBundleContentTypes.Contains(modelBundleFile.ContentType))
            return this.ValidationProblem("Unsupported file received as asset bundle");

        await using var modelImageStream = modelImageFile.OpenReadStream();
        var imageExtension = modelImageStream.GetExtension();
        if (!SupportedImageExtensions.Contains(imageExtension))
            return this.ValidationProblem("Unsupported image type received");

        await using var modelBundleStream = modelBundleFile.OpenReadStream();
        if (modelBundleStream.GetExtension() != Unity3dExtension)
            return this.ValidationProblem("Unsupported file received as asset bundle");

        var createModelResult = await this.modelsRepository.CreateModelAsync(
                modelName,
                modelGroup,
                modelImageStream,
                imageExtension,
                modelBundleStream)
            .ConfigureAwait(false);

        return createModelResult.TryGetFault(out var fault, out var result)
            ? this.ConvertFaultToActionResult(fault)
            : this.Created(new Uri($"https://www.argarden.ml/api/models/bundles/{result.Id}/0"), result);
    }

    [HttpPatch("patch/{modelId:guid}")]
    public async Task<ActionResult> PatchModelAsync(Guid modelId, [FromForm] PatchModelRequest request)
    {
        var (modelName, modelGroup, modelImageFile, modelBundleFile) = request;
        if (!SupportedImageContentTypes.Contains(modelImageFile.ContentType))
            return this.ValidationProblem("Unsupported image type received");

        if (!SupportedAssetBundleContentTypes.Contains(modelBundleFile.ContentType))
            return this.ValidationProblem("Unsupported file received as asset bundle");

        await using var modelImageStream = modelImageFile.OpenReadStream();
        var imageExtension = modelImageStream.GetExtension();
        if (!SupportedImageExtensions.Contains(imageExtension))
            return this.ValidationProblem("Unsupported image type received");

        await using var modelBundleStream = modelBundleFile.OpenReadStream();
        if (modelBundleStream.GetExtension() != Unity3dExtension)
            return this.ValidationProblem("Unsupported file received as asset bundle");

        var updateModelResult = await this.modelsRepository.UpdateModelAsync(
                modelId,
                modelName,
                modelGroup,
                modelImageStream,
                imageExtension,
                modelBundleStream)
            .ConfigureAwait(false);

        return updateModelResult.TryGetFault(out var fault)
            ? this.ConvertFaultToActionResult(fault)
            : this.Ok();
    }

    [HttpDelete("delete/{modelId:guid}")]
    public async Task<ActionResult> DeleteModelAsync(Guid modelId)
    {
        this.logger.LogInformation($"Deleting model with id: {modelId}");
        var removeResult = await this.modelsRepository.RemoveModelAsync(modelId).ConfigureAwait(false);
        return removeResult.TryGetFault(out var fault)
            ? this.ConvertFaultToActionResult(fault)
            : this.NoContent();
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