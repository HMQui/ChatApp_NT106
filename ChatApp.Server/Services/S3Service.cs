using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;

namespace ChatApp.Server.Services
{
    public class S3Service
    {
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;

        public S3Service(IConfiguration configuration)
        {
            var awsOptions = configuration.GetSection("AWS");
            var accessKey = awsOptions["AccessKey"];
            var secretKey = awsOptions["SecretKey"];
            _bucketName = awsOptions["BucketName"];
            var region = RegionEndpoint.GetBySystemName(awsOptions["Region"]);

            _s3Client = new AmazonS3Client(accessKey, secretKey, region);
        }

        public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName)
        {
            return await UploadFileInternalAsync(imageBytes, fileName, "image/jpeg");
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName)
        {
            return await UploadFileInternalAsync(fileBytes, fileName, "application/octet-stream");
        }

        private async Task<string> UploadFileInternalAsync(byte[] fileBytes, string fileName, string contentType)
        {
            using var stream = new MemoryStream(fileBytes);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            string url = $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
            return url;
        }
    }
}
