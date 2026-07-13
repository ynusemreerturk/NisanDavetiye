namespace NisanDavetiye.BLL.Security;

public static class ImageFileValidator
{
    public static bool TryDetect(string contentType, Stream stream, out string detectedType)
    {
        detectedType = string.Empty;
        if (!stream.CanRead)
            return false;

        if (stream.CanSeek)
            stream.Position = 0;

        Span<byte> header = stackalloc byte[16];
        var read = stream.Read(header);
        if (stream.CanSeek)
            stream.Position = 0;

        if (read < 4)
            return false;

        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            detectedType = "image/jpeg";
            return IsCompatible(contentType, detectedType);
        }

        if (read >= 8
            && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
        {
            detectedType = "image/png";
            return IsCompatible(contentType, detectedType);
        }

        if (read >= 12
            && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
        {
            detectedType = "image/webp";
            return IsCompatible(contentType, detectedType);
        }

        if (read >= 12 && header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70)
        {
            var brand = System.Text.Encoding.ASCII.GetString(header.Slice(8, 4));
            if (brand is "heic" or "heix" or "hevc" or "mif1" or "msf1" or "heif")
            {
                detectedType = brand is "heif" or "mif1" or "msf1" ? "image/heif" : "image/heic";
                return IsCompatible(contentType, detectedType);
            }
        }

        return false;
    }

    private static bool IsCompatible(string declared, string detected) =>
        string.Equals(declared, detected, StringComparison.OrdinalIgnoreCase)
        || (declared.Equals("image/heif", StringComparison.OrdinalIgnoreCase)
            && detected.Equals("image/heic", StringComparison.OrdinalIgnoreCase))
        || (declared.Equals("image/heic", StringComparison.OrdinalIgnoreCase)
            && detected.Equals("image/heif", StringComparison.OrdinalIgnoreCase));
}
