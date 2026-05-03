namespace PitStop.Application.Interfaces;

public interface IFileStorage
{
    /// <summary>
    ///     Uploads a file and returns its relative URL path (e.g. /uploads/shops/photo.jpg).
    /// </summary>
    Task<string> UploadAsync(Stream file, string fileName, string folder);

    /// <summary>
    ///     Deletes a file by its relative or absolute URL path.
    /// </summary>
    Task DeleteAsync(string url);
}