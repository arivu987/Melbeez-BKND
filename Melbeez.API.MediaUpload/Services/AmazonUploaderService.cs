using Amazon.S3;
using Amazon.S3.Transfer;
using Melbeez.MediaUpload.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Melbeez.MediaUpload.Services
{
    public class AmazonUploaderService
    {
        #region Document Upload

        public static async Task<bool> DocumentUpload(Stream ms, string fileName, ConfugurationModel confugurationModel)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(confugurationModel._BucketName) 
                    && !string.IsNullOrWhiteSpace(confugurationModel._AccessKey) 
                    && !string.IsNullOrWhiteSpace(confugurationModel._SecretKey))
                {
                    // Create request payload for AmazonS3.
                    TransferUtilityUploadRequest request = new TransferUtilityUploadRequest
                    {
                        BucketName = confugurationModel._BucketName,
                        Key = fileName,
                        InputStream = ms,
                        CannedACL = S3CannedACL.PublicRead,
                    };

                    Amazon.RegionEndpoint s3Region = Amazon.RegionEndpoint.USWest1;
                    IAmazonS3 s3Client = new AmazonS3Client(confugurationModel._AccessKey, confugurationModel._SecretKey, s3Region);
                    TransferUtility utility = new TransferUtility(s3Client);
                    utility.Upload(request);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion
    }
}
