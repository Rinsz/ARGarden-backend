namespace ThreeXyNine.ARGarden.Api.Models;

public record CreateModelRequest(string ModelName, ModelGroup ModelGroup, IFormFile ModelImage, IFormFile ModelBundle);