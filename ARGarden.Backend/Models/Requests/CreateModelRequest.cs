namespace ThreeXyNine.ARGarden.Api.Models;

public record CreateModelRequest(Guid Id, string ModelName, ModelGroup ModelGroup, IFormFile ModelImageFile, IFormFile ModelBundleFile);