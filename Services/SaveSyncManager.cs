using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class SaveSyncManager
    {
        private readonly DriveSyncService _driveSyncService;

        public SaveSyncManager(DriveSyncService driveSyncService)
        {
            _driveSyncService = driveSyncService;
        }

        public async Task<string> UploadSaveAsync(List<string> filePaths, string folderId, string gameName)
        {
            var existingNames = await _driveSyncService.GetFileNamesAsync(folderId);
            var driveFileName = SaveVersionNamer.BuildFileName(existingNames, gameName);

            string zipPath = Path.Combine(Path.GetTempPath(), driveFileName);
            SaveArchiver.CreateZip(filePaths, zipPath);

            try
            {
                return await _driveSyncService.UploadFileAsync(zipPath, driveFileName, folderId);
            }
            finally
            {
                File.Delete(zipPath);
            }
        }

        public async Task DownloadSaveAsync(string fileId, string localSavePath)
        {
            string zipPath = Path.Combine(Path.GetTempPath(), $"download_{fileId}.zip");

            try
            {
                await _driveSyncService.DownloadFileAsync(fileId, zipPath);
                SaveArchiver.ExtractZip(zipPath, localSavePath);
            }
            finally
            {
                File.Delete(zipPath);
            }
        }

        public Task<string> GetOrCreateFolderAsync(string folderName) =>
            _driveSyncService.GetOrCreateFolderAsync(folderName);

        public Task<List<Google.Apis.Drive.v3.Data.File>> GetRecentSavesAsync(string folderId, int count = 5) =>
            _driveSyncService.GetRecentSavesAsync(folderId, count);
    }
}
