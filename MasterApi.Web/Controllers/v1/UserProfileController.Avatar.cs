using MasterApi.Core.Account.Enums;
using MasterApi.Core.Extensions;
using MasterApi.Core.ViewModels.UserProfile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MasterApi.Web.Controllers.v1
{
 
    public partial class ProfileController : BaseController
    {
        [HttpPatch("avatar")]
        public async Task<IActionResult> UpdateAvatarAsync()
        {
            var httpRequest = HttpContext.Request;
            var files = httpRequest.Form.Files as List<IFormFile>;
            if (files.Count == 0)
            {
                return BadRequest("No files to be uploaded");
            }

            var file = files[0];
            var stream = file.OpenReadStream();
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

            var avatarUrl = await UploadFileAsBlob(stream, filename, "myuploads");

            var avatar = await _userProfileService.UpdatePhotoAsync(UserInfo.UserId, UserInfo.Username, filename);

            return avatar == null ?
                   NotFound(UserAccountMessages.UserNotFound.GetDescription()) :
                   Ok(new { Avatar = avatarUrl, Size = file.Length });
        }

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
