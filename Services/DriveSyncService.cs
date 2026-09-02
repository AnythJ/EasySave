using Google.Apis.Drive.v3;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class DriveSyncService
    {
        private readonly DriveService _service;

        public DriveSyncService(DriveService service)
        {
            _service = service;
        }

        public async Task<string> GetOrCreateFolderAsync(string folderName, string? parentId = null)
        {
            var query = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false";
            if (parentId != null)
                query += $" and '{parentId}' in parents";

            var listRequest = _service.Files.List();
            listRequest.Q = query;
            listRequest.Fields = "files(id, name)";
            var result = await listRequest.ExecuteAsync();

            if (result.Files.Count > 0)
                return result.Files[0].Id;

            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = parentId != null ? new[] { parentId } : null
            };

            var createRequest = _service.Files.Create(folderMetadata);
            createRequest.Fields = "id";
            var folder = await createRequest.ExecuteAsync();
            return folder.Id;
        }

        public async Task<List<string>> GetFileNamesAsync(string folderId)
        {
            var listRequest = _service.Files.List();
            listRequest.Q = $"'{folderId}' in parents and trashed=false";
            listRequest.Fields = "files(name)";
            var result = await listRequest.ExecuteAsync();
            return result.Files.Select(f => f.Name).ToList();
        }

        public async Task<string> UploadFileAsync(string localFilePath, string driveFileName, string folderId)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = driveFileName,
                Parents = new[] { folderId }
            };

            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            var uploadRequest = _service.Files.Create(fileMetadata, fileStream, "application/zip");
            uploadRequest.Fields = "id";
            await uploadRequest.UploadAsync();

            return uploadRequest.ResponseBody.Id;
        }

        public async Task<List<Google.Apis.Drive.v3.Data.File>> GetRecentSavesAsync(string folderId, int count = 5)
        {
            var listRequest = _service.Files.List();
            listRequest.Q = $"'{folderId}' in parents and trashed=false";
            listRequest.Fields = "files(id, name, createdTime)";
            listRequest.OrderBy = "createdTime desc";
            listRequest.PageSize = count;

            var result = await listRequest.ExecuteAsync();
            return result.Files.ToList();
        }

        public async Task DownloadFileAsync(string fileId, string destinationZipPath)
        {
            using var fileStream = new FileStream(destinationZipPath, FileMode.Create, FileAccess.Write);
            await _service.Files.Get(fileId).DownloadAsync(fileStream);
        }
    }
}
