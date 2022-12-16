using ThreeXyNine.ARGarden.Api.Abstractions;
using ThreeXyNine.ARGarden.Api.Settings;

namespace ThreeXyNine.ARGarden.Api.Providers;

[PrimaryConstructor]
public partial class PublicPropertiesProvider : IPublicPropertiesProvider
{
    private readonly IConfiguration configuration;

    public PublicProperties Get() =>
        this.configuration.GetRequiredSection($"{nameof(PublicProperties)}").Get<PublicProperties>();
}