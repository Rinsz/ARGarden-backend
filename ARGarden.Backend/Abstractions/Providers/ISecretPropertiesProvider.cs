using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Abstractions;

public interface ISecretPropertiesProvider
{
    SecretProperties Get();
}