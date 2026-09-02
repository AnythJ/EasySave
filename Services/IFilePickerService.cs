using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public interface IFilePickerService
    {
        Task<List<string>> PickFilesAsync(string title);
        Task<string?> PickFolderAsync(string title);
    }
}
