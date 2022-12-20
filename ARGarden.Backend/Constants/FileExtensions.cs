namespace ThreeXyNine.ARGarden.Api.Constants;

internal static class FileExtensions
{
    internal const string Unity3dExtension = ".unity3d";
    internal const string JpgExtension = ".jpg";
    internal const string JpegExtension = ".jpeg";
    internal const string PngExtension = ".png";

    internal static readonly IReadOnlySet<string> SupportedImageExtensions = new HashSet<string>
    {
        JpegExtension,
        JpgExtension,
        PngExtension,
    };

    internal static string ImageExtensionErrorMessage =>
        $"Image file should have one of extensions: {string.Join("; ", SupportedImageExtensions)}";

    internal static string BundleExtensionErrorMessage =>
        $"AssetBundle file should have extension: {Unity3dExtension}";
}