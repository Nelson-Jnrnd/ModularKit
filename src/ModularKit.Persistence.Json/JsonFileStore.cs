using ModularKit.Persistence.Json;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ModularKit.Persistence.Json;

public sealed class JsonFileStore(ILoggerFactory loggerFactory) : IJsonFileStore
{
    private readonly ILogger<JsonFileStore> _logger = loggerFactory.CreateLogger<JsonFileStore>();

    public async Task<T> LoadAsync<T>(FileInfo path, JsonSerializerOptions options, CancellationToken ct) where T : class
    {
        try
        {
            string json = await File.ReadAllTextAsync(path.FullName, ct);
            T value = JsonSerializer.Deserialize<T>(json, options)
                ?? throw new ApplicationException("Failed to deserialize from JSON.");
            return value;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            _logger.LogError(ex, "Failed to read from {FilePath}.", path.FullName);
            throw new ApplicationException($"Error reading from file '{path.FullName}': {ex.Message}", ex);
        }
    }

    public async Task SaveAsync<T>(T value, FileInfo path, JsonSerializerOptions options, CancellationToken ct) where T : class
    {
        // Write to a sibling temp file first, then atomically swap it into place. This guarantees the
        // existing (good) file is never left half-written: a crash or partial write only ever damages the
        // throwaway temp file, and the live file is replaced in a single atomic operation on the same volume.
        string directory = path.DirectoryName!;
        string tempPath = Path.Combine(directory, path.Name + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            Directory.CreateDirectory(directory);
            string json = JsonSerializer.Serialize(value, options);
            await using (FileStream stream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes(json), ct);
                await stream.FlushAsync(ct);
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(path.FullName))
                File.Replace(tempPath, path.FullName, path.FullName + ".bak");
            else
                File.Move(tempPath, path.FullName, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Failed to write to {FilePath}.", path.FullName);
            throw new ApplicationException($"Error writing to file '{path.FullName}': {ex.Message}", ex);
        }
        finally
        {
            TryDeleteTempFile(tempPath);
        }
    }

    private void TryDeleteTempFile(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Failed to remove temporary file {TempPath}.", tempPath);
        }
    }
}

