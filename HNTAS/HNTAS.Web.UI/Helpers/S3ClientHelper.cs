using Amazon.S3;

namespace HNTAS.Web.UI.Helpers
{
    public class S3ClientHelper
    {
        public static IAmazonS3 Create(IConfiguration config)
        {
            var useLocal = config.GetValue<bool>("AWS:UseLocalStack");

            if (useLocal)
            {
                var localUrl = config["AWS:LocalStackUrl"];
                var s3Config = new AmazonS3Config
                {
                    ServiceURL = localUrl,
                    ForcePathStyle = true
                };

                return new AmazonS3Client("test", "test", s3Config); // LocalStack creds
            }

            return new AmazonS3Client();
        }
    }
}
