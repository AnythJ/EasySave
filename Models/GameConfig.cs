using System.Collections.Generic;

namespace EasySave.Models
{
    public class GameConfig
    {
        public string Name { get; set; } = string.Empty;
        public string DownloadDestinationPath { get; set; } = string.Empty;
        public List<string> LastSelectedFiles { get; set; } = new();
        public string? LastKnownDriveFileId { get; set; }
    }
}
