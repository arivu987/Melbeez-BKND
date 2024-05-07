using Melbeez.MediaUpload.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Melbeez.MediaUpload.Services
{
    public class MediaUploadModel
    {
        public string DirToSave { get; set; }
        public IFormFile File { get; set; }
        public string BaseDirPath { get; set; }
    }

    public class FileUploadResponseService
    {
        public async Task<FileUploadResponse> Save(MediaUploadModel mediaUploadModel,ConfugurationModel confugurationModel)
        {
            FileUploadResponse result = new FileUploadResponse();
            try
            {
                var TargetDir = Path.Combine(mediaUploadModel.BaseDirPath ?? "", mediaUploadModel.DirToSave);

                if (!Directory.Exists(TargetDir))
                {
                    Directory.CreateDirectory(TargetDir);
                }

                string FileEx = Path.GetExtension(mediaUploadModel.File.FileName);
                var newFileName = Guid.NewGuid() + FileEx;

                if (confugurationModel._FileExtentions.Contains(FileEx))
                {
                    using (var ms = new MemoryStream())
                    {
                        await mediaUploadModel.File.CopyToAsync(ms);

                        if(confugurationModel._UploadTo == "AWSS3")
                        {
                            result.IsSuccess = await AWSS3Upload(ms, mediaUploadModel.DirToSave + @"/" + newFileName, confugurationModel);
                        }
                        else
                        {
                            System.IO.File.WriteAllBytes(TargetDir + Path.DirectorySeparatorChar + newFileName, ms.ToArray());
                            result.IsSuccess = true;
                        }
                    }
                }

                if (result.IsSuccess)
                {
                    result.FileUrl = mediaUploadModel.DirToSave + @"/" + newFileName;
                    result.FileSize = Convert.ToInt32(mediaUploadModel.File.Length);
                }
            }
            catch (Exception ex)
            {
                if (ex != null && ex.InnerException != null)
                {
                    result.Message = "inner" + ex.InnerException.Message;
                }
                else
                {
                    result.Message = "msg" + ex.Message;
                }
                result.IsSuccess = false;
            }

            return result;
        }

        private async Task<bool> AWSS3Upload(Stream stream, string fileName, ConfugurationModel confugurationModel)
        {
            var isSuccess = await AmazonUploaderService.DocumentUpload(stream, fileName, confugurationModel);
            if (isSuccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
