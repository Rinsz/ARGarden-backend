using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Providers;

[PrimaryConstructor]
public partial class SecretPropertiesProvider : ISecretPropertiesProvider
{
    private readonly IConfiguration configuration;

    public SecretProperties Get() =>
        this.configuration.GetRequiredSection($"{nameof(SecretProperties)}").Get<SecretProperties>();
}