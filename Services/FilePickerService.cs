using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class FilePickerService : IFilePickerService
    {
        private Avalonia.Controls.TopLevel? GetTopLevel()
        {
            return App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? Avalonia.Controls.TopLevel.GetTopLevel(desktop.MainWindow)
                : null;
        }

        public async Task<List<string>> PickFilesAsync(string title)
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return new List<string>();

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true
            });

            return files.Select(f => f.Path.LocalPath).ToList();
        }

        public async Task<string?> PickFolderAsync(string title)
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return null;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            return folders.Count > 0 ? folders[0].Path.LocalPath : null;
        }
    }
}
