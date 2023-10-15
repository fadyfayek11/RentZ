using ExtCore.FileStorage.Abstractions;
using Microsoft.AspNetCore.Http;

namespace RentZ.Application.Services.Files;
public interface IFileManager
{
    Task<bool> SaveFileAsync<T>(IFormFile file, string fileName, string path, string? folderName = null);
    Task<bool> UpdateFileAsync<T>(IFormFile file, string oldFileName, string newFileName, string path);
    Task<IFileProxy> FileProxy<T>(string fileName, string path);
    string GetContentType(string fileName);
};
public class FileManager : IFileManager
{
    readonly IFileStorage _fileStorage;
    readonly IContentTypeProvider _contentTypeProvider;
    public FileManager(IFileStorage fileStorage, IContentTypeProvider contentTypeProvider)
    {
        _fileStorage = fileStorage;
        _contentTypeProvider = contentTypeProvider;
    }

    public async Task<bool> UpdateFileAsync<T>(IFormFile file, string oldFileName, string newFileName, string path)
    {
        if (!string.IsNullOrEmpty(oldFileName))
        {
            var a = await FileProxy<T>(oldFileName, path);
            if (await a.ExistsAsync())
            {
                await a.DeleteAsync();
            }
        }
        return await SaveFileAsync<T>(file, newFileName, path);
    }
    public async Task<bool> SaveFileAsync<T>(IFormFile file, string fileName, string path, string? folderName = null)
    {
        var _ = folderName ?? typeof(T).Name;
        try
        {
            var directoryProxy = _fileStorage.CreateDirectoryProxy($"{_}\\{path}");
            await directoryProxy.CreateAsync();

            var fileProxy = _fileStorage.CreateFileProxy(directoryProxy.RelativePath, fileName);

            await fileProxy.WriteStreamAsync(file.OpenReadStream());
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<IFileProxy> FileProxy<T>(string fileName, string path)
    {
        var _ = typeof(T).Name;
        var directoryProxy = _fileStorage.CreateDirectoryProxy($"\\{_}\\{path}");
        if (!await directoryProxy.ExistsAsync())
            await directoryProxy.CreateAsync();
        return _fileStorage.CreateFileProxy(directoryProxy.RelativePath, fileName);
    }
    public string GetContentType(string fileName)
    {
        string contentType;
        if (!_contentTypeProvider.TryGetContentType(fileName, out contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

}

