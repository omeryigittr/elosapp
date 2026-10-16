namespace ElosApp.Services;

public interface IPhotoStorageService
{
    Task<string> SaveOptimizedAsync(FileResult photo, CancellationToken cancellationToken = default);
    Task DeleteAsync(string? path);
}
