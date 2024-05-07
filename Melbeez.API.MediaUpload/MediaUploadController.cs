using Melbeez.MediaUpload.Models;
using Melbeez.MediaUpload.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.MediaUpload
{
    [Route("media")]
    public class MediaUploadController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public MediaUploadController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 629145600)]
        public async Task<IActionResult> Upload([FromForm(Name = "files")] IFormFile[] files, [FromForm(Name = "fileDriveName")] string fileDriveName)
        {
            if (files.Count() < 0)
            {
                return Ok();
            }

            var isApiAuthorized = configuration.GetValue<bool>("MediaUploadConfiguration:IsApiAuthorized");
            if (isApiAuthorized)
            {
                var isAuthenticated = User.Identity?.IsAuthenticated;
                if (isAuthenticated.HasValue && isAuthenticated == false)
                {
                    return new UnauthorizedObjectResult("Access denied");
                }
            }

            List<FileUploadResponse> result = new List<FileUploadResponse>();
            var baseDirPath = configuration.GetValue<string>("MediaUploadConfiguration:BasePath");
            var uploadTo = configuration.GetValue<string>("MediaUploadConfiguration:UploadTo");
            var confugurationModel = new ConfugurationModel()
            {
                _AccessKey = configuration.GetValue<string>("MediaUploadConfiguration:AWSS3Configuration:AccessKey"),
                _SecretKey = configuration.GetValue<string>("MediaUploadConfiguration:AWSS3Configuration:SecretKey"),
                _BucketName = configuration.GetValue<string>("MediaUploadConfiguration:AWSS3Configuration:BucketName"),
                _UploadTo = uploadTo,
                _FileExtentions = configuration.GetValue<string>("MediaUploadConfiguration:FileExtention").Split(",").ToList()
            };

            if (confugurationModel._AccessKey == null && confugurationModel._SecretKey == null && confugurationModel._BucketName == null)
            {
                return new BadRequestObjectResult("Key not found");
            }

            FileUploadResponseService fileUploadService = new FileUploadResponseService();
            foreach (var item in files)
            {
                var response = await fileUploadService.Save(new MediaUploadModel()
                {
                    DirToSave = fileDriveName,
                    File = item,
                    BaseDirPath = baseDirPath
                },confugurationModel);

                result.Add(response);
            }
            return Ok(result);
        }
    }
}
