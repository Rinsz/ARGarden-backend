using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface IModelsRepositorySettingsProvider
{
    ModelsRepositorySettings Get();
}