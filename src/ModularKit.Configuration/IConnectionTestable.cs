namespace ModularKit.Configuration;

/// <summary>
/// A configurable component whose connectivity can be probed on demand from the config UI, without
/// leaving a permanent connection open.
/// </summary>
public interface IConnectionTestable
{
    /// <summary>
    /// Tests connectivity and returns a detailed result without leaving a permanent connection open.
    /// May throw <see cref="NotSupportedException"/> when the concrete type does not support testing.
    /// </summary>
    Task<ConnectionTestResult> TestConnectionAsync(CancellationToken ct);
}

