namespace ThreeXyNine.ARGarden.Api.Constants;

internal static class ContentTypes
{
    public const string AssetBundleContentType = "application/unity3d";
    public const string OctetStreamContentType = "application/octet-stream";
    public const string JpegContentType = "image/jpeg";
    public const string JpgContentType = "image/jpg";
    public const string PngContentType = "image/png";

    internal static readonly IReadOnlySet<string> SupportedAssetBundleContentTypes = new HashSet<string>
    {
        AssetBundleContentType,
        OctetStreamContentType,
    };

    internal static readonly IReadOnlySet<string> SupportedImageContentTypes = new HashSet<string>
    {
        JpegContentType,
        JpgContentType,
        PngContentType,
    };

    internal static string ImageContentTypeErrorMessage =>
        $"Image file should have one of content-types: {string.Join("; ", SupportedImageContentTypes)}";

    internal static string BundleContentTypeErrorMessage =>
        $"AssetBundle file should have one of content-types: {string.Join("; ", SupportedAssetBundleContentTypes)}";
}