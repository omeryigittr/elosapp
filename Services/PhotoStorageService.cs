using Microsoft.Maui.Storage;

namespace ElosApp.Services;

public sealed class PhotoStorageService : IPhotoStorageService
{
    public async Task<string> SaveOptimizedAsync(FileResult photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);
        var mediaDirectory = Path.Combine(FileSystem.AppDataDirectory, "media");
        Directory.CreateDirectory(mediaDirectory);
        var destination = Path.Combine(mediaDirectory, $"photo-{Guid.NewGuid():N}.jpg");

#if ANDROID
        await using var source = await photo.OpenReadAsync();
        using var bitmap = Android.Graphics.BitmapFactory.DecodeStream(source);
        if (bitmap is null)
            throw new InvalidDataException("Fotoğraf okunamadı.");

        using var output = File.Create(destination);
        bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg!, 78, output);
#elif IOS || MACCATALYST
        await using var source = await photo.OpenReadAsync();
        using var data = Foundation.NSData.FromStream(source);
        using var image = UIKit.UIImage.LoadFromData(data);
        if (image is null)
            throw new InvalidDataException("Fotoğraf okunamadı.");

        using var jpeg = image.AsJPEG(.78f);
        if (jpeg is null || !jpeg.Save(destination, true))
            throw new IOException("Fotoğraf kaydedilemedi.");
#else
        await using var source = await photo.OpenReadAsync();
        await using var target = File.Create(destination);
        await source.CopyToAsync(target, cancellationToken);
#endif

        return destination;
    }

    public Task DeleteAsync(string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }
}
