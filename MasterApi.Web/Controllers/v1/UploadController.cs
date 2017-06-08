using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Config;
using MasterApi.Core.Infrastructure.Storage;
using MasterApi.Web.Filters;
using System.Linq;

namespace MasterApi.Web.Controllers.v1
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MasterApi.Web.Controllers.BaseController" />
    [Route("api/{version}/[controller]")]
    public class UploadController : BaseController
    {
        private readonly IHostingEnvironment _env;
        private readonly AppSettings _settings;
        private readonly BlobStorageProviderSettings _blobSettings;
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailController" /> class.
        /// </summary>
        public UploadController(IUserInfo userinfo, IOptions<AppSettings> settings, IHostingEnvironment environment) : base(userinfo)
        {
            _env = environment;
            _settings = settings.Value;
            _blobSettings =
                _settings.BlobStorageProviders.FirstOrDefault(x => x.Provider == _settings.DefaultStorageProvider);
        }

        /// <summary>
        /// Uploads the asynchronous.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ValidationException">No asp.net core 1.0 support yet</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        [HttpPost("")]
        [ServiceFilter(typeof(ValidateMimeMultipartContentFilter))]
        public async Task<IActionResult> UploadAsync() 
        {
            var httpRequest = HttpContext.Request;
            var files = httpRequest.Form.Files as List<IFormFile>;

            if (files == null)
            {
                return BadRequest("No files to be uploaded");
            }

            var uploads = Path.Combine(_env.WebRootPath, "Uploads");
            long size = 0;

            foreach (var file in files)
            {
                if (file.Length <= 0) continue;
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                size += file.Length;
                using (var fileStream = new FileStream(Path.Combine(uploads, filename), FileMode.Create))
                {
                    switch (_settings.DefaultStorageProvider)
                    {
                        case BlobStorageProvider.FileSystem :
                            await file.CopyToAsync(fileStream);
                            break;
                        case BlobStorageProvider.Azure:
                            await UploadFileAsBlob(fileStream, filename, "myuploads");
                            break;
                        case BlobStorageProvider.Cloudinary:
                            throw new ValidationException("No asp.net core 1.0 support yet");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return Ok(new { Message = $"{files.Count} file(s) / { size} bytes uploaded successfully!" });
        }

        /// <summary>
        /// Upload file in azure storage
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <returns></returns>
        [HttpPost("blob")]
        public async Task<string> UploadFileAsBlob(Stream stream, string filename, string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(_blobSettings.ApiBaseUrl);

            if (storageAccount == null) return null;
            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            var container = blobClient.GetContainerReference(containerName);

            if (await container.CreateIfNotExistsAsync())
            {
                // configure container for public access
                var permissions = await container.GetPermissionsAsync();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                await container.SetPermissionsAsync(permissions);
            }

            var blockBlob = container.GetBlockBlobReference(filename);

            await blockBlob.UploadFromStreamAsync(stream);

            stream.Dispose();

            return blockBlob.Uri.ToString();
        }
    }
}
