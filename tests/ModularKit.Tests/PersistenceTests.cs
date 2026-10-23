using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using ModularKit.Persistence.Json;
using Xunit;

namespace ModularKit.Tests;

public class PersistenceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "modularkit-tests", Guid.NewGuid().ToString("N"));
    private readonly IJsonFileStore _store = new JsonFileStore(NullLoggerFactory.Instance);
    private readonly JsonSerializerOptions _options = new();
    private FileInfo File => new(Path.Combine(_directory,"settings.json"));

    [Fact]
    public async Task Replacement_preserves_previous_contents_in_backup()
    {
        await _store.SaveAsync(new Settings("first"), File, _options, CancellationToken.None);
        await _store.SaveAsync(new Settings("second"), File, _options, CancellationToken.None);
        Assert.Equal("second", (await _store.LoadAsync<Settings>(File, _options, CancellationToken.None)).Value);
        Assert.Equal("first", (await _store.LoadAsync<Settings>(new FileInfo(File.FullName+".bak"), _options, CancellationToken.None)).Value);
        Assert.Empty(Directory.GetFiles(_directory,"*.tmp"));
    }

    [Fact]
    public async Task Cancelled_write_keeps_existing_file_and_removes_temporary_files()
    {
        await _store.SaveAsync(new Settings("first"), File, _options, CancellationToken.None);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _store.SaveAsync(new Settings("second"), File, _options, cancellation.Token));
        Assert.Equal("first", (await _store.LoadAsync<Settings>(File, _options, CancellationToken.None)).Value);
        Assert.Empty(Directory.GetFiles(_directory,"*.tmp"));
    }

    [Fact]
    public async Task Invalid_json_is_reported_and_not_overwritten()
    {
        Directory.CreateDirectory(_directory);
        await System.IO.File.WriteAllTextAsync(File.FullName,"invalid json");
        var error = await Assert.ThrowsAsync<ApplicationException>(() => _store.LoadAsync<Settings>(File,_options,CancellationToken.None));
        Assert.IsAssignableFrom<JsonException>(error.InnerException);
        Assert.Equal("invalid json", await System.IO.File.ReadAllTextAsync(File.FullName));
    }

    public void Dispose() { if (Directory.Exists(_directory)) Directory.Delete(_directory,true); }
    public sealed record Settings(string Value);
}
