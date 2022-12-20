using Microsoft.AspNetCore.Mvc;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Models;

namespace ThreeXyNine.ARGarden.Api.Controllers;

[ApiController]
[Route("api/admin/models")]
public class ModelsAdminController : ControllerBase
{
    private const string AssetBundleContentType = "application/unity3d";

    private static readonly IReadOnlySet<string> SupportedImageContentTypes = new HashSet<string>
    {
        "image/jpg",
        "image/jpeg",
        "image/png",
    };

    private readonly IModelsRepository modelsRepository;
    private readonly ILogger<ModelsAdminController> logger;

    public ModelsAdminController(IModelsRepository modelsRepository, ILogger<ModelsAdminController> logger)
    {
        this.modelsRepository = modelsRepository;
        this.logger = logger;
    }

    [HttpPost("new")]
#pragma warning disable SA1313
    public async Task<ActionResult> CreateModelAsync(CreateModelRequest request, IFormFile _)
#pragma warning restore SA1313
    {
        var (modelName, modelGroup, modelImage, modelBundle) = request;
        if (!SupportedImageContentTypes.Contains(modelImage.ContentType))
        {
            return this.ValidationProblem("Unsupported image type received");
        }

        if (modelBundle.ContentType != AssetBundleContentType)
        {
            return this.ValidationProblem("Unsupported file received as asset bundle");
        }

        await using var modelImageStream = modelImage.OpenReadStream();
        await using var modelBundleStream = modelBundle.OpenReadStream();
        var createModelResult = await this.modelsRepository.CreateModelAsync(modelName, modelGroup, modelImageStream, modelBundleStream)
            .ConfigureAwait(false);

        return createModelResult.TryGetFault(out var fault, out var result)
            ? this.ConvertFaultToActionResult(fault)
            : this.Created(new Uri($"api/models/bundles/{result.Id}/0"), result);
    }

    [HttpPatch("patch/{modelId:guid}")]
#pragma warning disable SA1313
    public async Task<ActionResult> PatchModelAsync(Guid modelId, PatchModelRequest request, IFormFile _)
#pragma warning restore SA1313
    {
        var (modelName, modelGroup, modelImageFile, modelBundleFile) = request;
        if (!SupportedImageContentTypes.Contains(modelImageFile.ContentType))
        {
            return this.ValidationProblem("Unsupported image type received");
        }

        if (modelBundleFile.ContentType != AssetBundleContentType)
        {
            return this.ValidationProblem("Unsupported file received as asset bundle");
        }

        await using var modelImageStream = modelImageFile.OpenReadStream();
        await using var modelBundleStream = modelBundleFile.OpenReadStream();
        var updateModelResult = await this.modelsRepository.UpdateModelAsync(modelId, modelName, modelGroup, modelImageStream, modelBundleStream)
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