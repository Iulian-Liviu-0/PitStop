using Microsoft.AspNetCore.Hosting;
using PitStop.Application.Interfaces;

namespace PitStop.Infrastructure.Storage;

public class LocalFileStorage(IWebHostEnvironment env) : IFileStorage
{
    private const string UploadsFolder = "uploads";

    /// <inheritdoc />
    public async Task<string> UploadAsync(Stream file, string fileName, string folder)
    {
        var safeFileName = $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}";
        var relativePath = Path.Combine(UploadsFolder, folder, safeFileName);
        var absolutePath = Path.Combine(env.WebRootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var stream = new FileStream(absolutePath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream);

        // Return URL-style path with forward slashes
        return "/" + relativePath.Replace('\\', '/');
    }

    /// <inheritdoc />
    public Task DeleteAsync(string url)
    {
        // Accept both relative URL (/uploads/shops/file.jpg) and absolute disk path
        var relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.IsPathRooted(url)
            ? url
            : Path.Combine(env.WebRootPath, relativePath);

        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        return Task.CompletedTask;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var name = string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));
        // Keep only the last 100 chars to avoid path length issues
        return name.Length > 100 ? name[^100..] : name;
    }
}