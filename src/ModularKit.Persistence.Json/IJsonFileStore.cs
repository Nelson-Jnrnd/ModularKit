using System.Text.Json;

namespace ModularKit.Persistence.Json;

public interface IJsonFileStore
{
    Task<T> LoadAsync<T>(FileInfo path, JsonSerializerOptions options, CancellationToken ct) where T : class;
    Task SaveAsync<T>(T value, FileInfo path, JsonSerializerOptions options, CancellationToken ct) where T : class;
}

