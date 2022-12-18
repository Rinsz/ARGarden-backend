using Microsoft.AspNetCore.Mvc;
using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Errors;

namespace ThreeXyNine.ARGarden.Api.Controllers;

[ApiController]
[Route("api/admin/models")]
public class ModelsAdminController : ControllerBase
{
    private readonly IModelsRepository modelsRepository;
    private readonly ILogger<ModelsAdminController> logger;

    public ModelsAdminController(IModelsRepository modelsRepository, ILogger<ModelsAdminController> logger)
    {
        this.modelsRepository = modelsRepository;
        this.logger = logger;
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