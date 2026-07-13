namespace NisanDavetiye.BLL.DTOs;

public record GaleriUploadResultDto(
    int UploadedCount,
    IReadOnlyList<string> FileNames,
    string Message);

public record GaleriSilResultDto(int DeletedCount);

public record GaleriUploadFile(string FileName, string ContentType, Stream Content);
