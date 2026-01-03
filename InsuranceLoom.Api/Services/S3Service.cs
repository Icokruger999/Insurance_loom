using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace InsuranceLoom.Api.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly IConfiguration _configuration;

    public S3Service(IConfiguration configuration)
    {
        _configuration = configuration;
        var awsConfig = configuration.GetSection("AWS");
        _bucketName = awsConfig["S3Bucket"] ?? "insurance-loom-documents";
        
        var region = Amazon.RegionEndpoint.GetBySystemName(awsConfig["Region"] ?? "af-south-1");
        _s3Client = new AmazonS3Client(
            awsConfig["AccessKey"],
            awsConfig["SecretKey"],
            region
        );
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath)
    {
        var key = $"{folderPath}/{fileName}";
        
        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = key,
            BucketName = _bucketName,
            CannedACL = S3CannedACL.Private,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest);

        return key;
    }

    public async Task<string> GetSignedUrlAsync(string filePath, int expirationMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = filePath,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };

        return await _s3Client.GetPreSignedURLAsync(request);
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = filePath
            };

            await _s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = filePath
        };

        var response = await _s3Client.GetObjectAsync(request);
        return response.ResponseStream;
    }
}

