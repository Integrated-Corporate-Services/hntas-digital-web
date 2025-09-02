using Amazon.S3;
using Amazon.S3.Transfer;

namespace HNTAS.Web.UI.Services
{
    public class S3UploadService : IS3UploadService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<S3UploadService> _logger;

        public S3UploadService(IAmazonS3 s3Client, IConfiguration config, ILogger<S3UploadService> logger)
        {
            _s3Client = s3Client;
            _bucketName = config["AWS:BucketName"];
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string keyPrefix)
        {
            var key = $"{keyPrefix}/{file.FileName}";
            using var stream = file.OpenReadStream();

            var request = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _bucketName,
                ContentType = file.ContentType
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(request);

            _logger.LogInformation("Uploaded file {FileName} to S3 at {Key}", file.FileName, key);
            return key;
        }

        public async Task<Stream?> GetFileAsync(string key)
        {
            try
            {
                var response = await _s3Client.GetObjectAsync(_bucketName, key);
                _logger.LogInformation("Retrieved file from S3: {Key}", key);

                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File not found in S3: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file from S3: {Key}", key);
                throw;
            }
        }
    }
}
