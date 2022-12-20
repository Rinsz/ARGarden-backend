namespace ThreeXyNine.ARGarden.Api.Models;

public record PatchModelRequest(string ModelName, ModelGroup ModelGroup, IFormFile ModelImageFile, IFormFile ModelBundleFile);