namespace ARGarden.Backend.Settings;

public class FileSystemRepositorySettingsProvider
{
    private static readonly string VariableName = $"argarden_{nameof(FileSystemRepositorySettings.StoragePath).ToLowerInvariant()}";

    private readonly IConfiguration configurationProvider;
    private readonly Lazy<FileSystemRepositorySettings> cachedPath;

    public FileSystemRepositorySettingsProvider(IConfiguration configurationProvider)
    {
        this.configurationProvider = configurationProvider;
        this.cachedPath = new(this.GetPrivate);
    }

    public FileSystemRepositorySettings Get() => this.cachedPath.Value;

    private FileSystemRepositorySettings GetPrivate()
    {
        var storagePath = this.configurationProvider.GetValue<string>(VariableName);

        return string.IsNullOrWhiteSpace(storagePath)
            ? throw new Exception($"Environment variable for file storage is not configured. VariableName: {VariableName}")
            : new(storagePath);
    }
}