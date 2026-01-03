namespace InsuranceLoom.Api.Services;

public interface IS3Service
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath);
    Task<string> GetSignedUrlAsync(string filePath, int expirationMinutes = 60);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream> DownloadFileAsync(string filePath);
}

