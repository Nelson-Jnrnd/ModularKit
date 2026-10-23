namespace ModularKit.Configuration;

/// <summary>
/// Result of an <see cref="IConnectionTestable.TestConnectionAsync"/> probe.
/// </summary>
public sealed record ConnectionTestResult(
    bool Success,
    string? Identifier,
    string? FirmwareVersion,
    TimeSpan ResponseTime,
    string? ErrorMessage);

