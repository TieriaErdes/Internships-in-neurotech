using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Internships_in_neurotech.Services;

public interface IFilesService
{
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFile?> SaveFileAsync();
    public Task<IStorageFolder?> GetFolderAsync();
}