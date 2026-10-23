using Microsoft.AspNetCore.Components.Server.Circuits;

namespace ModularKit.Configuration.Blazor.Infrastructure;

/// <summary>
/// Publishes the culture of the connection that opens a circuit to that circuit's <see cref="CircuitCulture"/>.
/// </summary>
/// <remarks>
/// The circuit is opened from its connection, which went through the request-localization middleware, so this is
/// the last point where the negotiated culture is still ambient and can be read without an
/// <see cref="Microsoft.AspNetCore.Http.HttpContext"/>.
/// </remarks>
internal sealed class CircuitCultureHandler : CircuitHandler
{
    private readonly CircuitCulture _circuitCulture;

    public CircuitCultureHandler(CircuitCulture circuitCulture)
    {
        _circuitCulture = circuitCulture;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitCulture.CaptureCurrent();
        return Task.CompletedTask;
    }

    /// <summary>
    /// A reconnection negotiates a culture of its own, and it is the current one from then on.
    /// </summary>
    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitCulture.CaptureCurrent();
        return Task.CompletedTask;
    }
}

