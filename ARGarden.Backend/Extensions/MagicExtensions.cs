using static ThreeXyNine.ARGarden.Api.Constants.FileExtensions;

namespace ThreeXyNine.ARGarden.Api.Extensions;

internal static class MagicExtensions
{
    internal static string GetExtension(this Stream fileStream)
    {
        var br = new BinaryReader(fileStream);
        var magicBytes = br.ReadBytes(0x10);
        var hexMagicBytesStr = BitConverter.ToString(magicBytes);
        var magicValue = hexMagicBytesStr[..11];

        fileStream.Position = 0;
        return magicValue switch
        {
            "FF-D8-FF-E1" => JpgExtension,
            "FF-D8-FF-E0" => JpegExtension,
            "89-50-4E-47" => PngExtension,
            "55-6E-69-74" => Unity3dExtension,
            _ => "unsupported",
        };
    }
}