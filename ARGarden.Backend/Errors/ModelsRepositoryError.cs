namespace ARGarden.Backend.Errors;

public record ModelsRepositoryError(ApiErrorType ErrorType, string Description);