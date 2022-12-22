using System.Net;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;
using ThreeXyNine.ARGarden.Api.Extensions;
using ThreeXyNine.ARGarden.Api.Models;

namespace ThreeXyNine.ARGarden.Api.Controllers;

[ApiController]
[Route("api/admin/models")]
public class ModelsAdminController : ControllerBase
{
    private readonly IValidator<CreateModelRequest> createModelRequestValidator;
    private readonly IValidator<PatchModelRequest> patchModelRequestValidator;
    private readonly IModelsRepository modelsRepository;
    private readonly ILogger<ModelsAdminController> logger;

    public ModelsAdminController(
        IValidator<CreateModelRequest> createModelRequestValidator,
        IValidator<PatchModelRequest> patchModelRequestValidator,
        IModelsRepository modelsRepository,
        ILogger<ModelsAdminController> logger)
    {
        this.modelsRepository = modelsRepository;
        this.logger = logger;
        this.createModelRequestValidator = createModelRequestValidator;
        this.patchModelRequestValidator = patchModelRequestValidator;
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateModelAsync([FromForm] CreateModelRequest request)
    {
        var validationResult = await this.createModelRequestValidator.ValidateAsync(request).ConfigureAwait(false);
        if (!validationResult.IsValid)
            return this.CreateValidationProblem(validationResult);

        var (modelName, modelGroup, modelImageFile, modelBundleFile) = request;

        await using var modelImageStream = modelImageFile.OpenReadStream();
        var imageExtension = modelImageStream.GetExtension();
        await using var modelBundleStream = modelBundleFile.OpenReadStream();

        var createModelResult = await this.modelsRepository.CreateModelAsync(
                modelName,
                modelGroup,
                modelImageStream,
                imageExtension,
                modelBundleStream)
            .ConfigureAwait(false);

        return createModelResult.TryGetFault(out var fault, out var result)
            ? this.ConvertFaultToActionResult(fault)
            : this.Created(new Uri($"https://{this.Request.Host}/api/models/bundles/{result.Id}/0"), result);
    }

    [HttpPatch("patch/{modelId:guid}")]
    public async Task<ActionResult> PatchModelAsync(Guid modelId, [FromForm] PatchModelRequest request)
    {
        var validationResult = await this.patchModelRequestValidator.ValidateAsync(request).ConfigureAwait(false);
        if (!validationResult.IsValid)
            return this.CreateValidationProblem(validationResult);
        var (modelName, modelGroup, modelImageFile, modelBundleFile) = request;

        await using var modelImageStream = modelImageFile.OpenReadStream();
        var imageExtension = modelImageStream.GetExtension();
        await using var modelBundleStream = modelBundleFile.OpenReadStream();

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

    private ActionResult CreateValidationProblem(ValidationResult validationResult)
    {
        var errors = validationResult.Errors.ToDictionary(
            error => error.PropertyName,
            error => new[] { error.ErrorMessage });

        return this.ValidationProblem(new ValidationProblemDetails(errors) { Status = (int)HttpStatusCode.UnprocessableEntity, });
    }
}