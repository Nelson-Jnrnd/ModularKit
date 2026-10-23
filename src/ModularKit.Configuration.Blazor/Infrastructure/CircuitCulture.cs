using System.Globalization;

namespace ModularKit.Configuration.Blazor.Infrastructure;

/// <summary>
/// The culture negotiated for the circuit's own connection, kept for the lifetime of that circuit.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CultureInfo.CurrentUICulture"/> and <see cref="CultureInfo.CurrentCulture"/> travel with the
/// execution context, and a work item dispatched onto the renderer runs under the context of whoever dispatched
/// it. Work coming from a user interaction is dispatched by the connection, which negotiated a culture; work
/// coming from a machine poll, a timer or a domain event is dispatched by a background thread that has none, so
/// it falls back to the process default. Both paths re-render the same components, and the rendered text then
/// alternates between two languages and two number formats.
/// </para>
/// <para>
/// One instance per circuit, so a server with several signed-in operators keeps one culture per session.
/// The ambient culture is used until the connection has published its own, which covers a static render.
/// </para>
/// </remarks>
public sealed class CircuitCulture
{
    private CultureInfo? _culture;
    private CultureInfo? _uiCulture;

    public CultureInfo Culture => _culture ?? CultureInfo.CurrentCulture;

    public CultureInfo UiCulture => _uiCulture ?? CultureInfo.CurrentUICulture;

    /// <summary>
    /// Adopts the culture in effect on the calling execution context. Called once, from the connection that
    /// owns the circuit, which is the only place the negotiated culture is still ambient.
    /// </summary>
    public void CaptureCurrent()
    {
        _culture = CultureInfo.CurrentCulture;
        _uiCulture = CultureInfo.CurrentUICulture;
    }

    /// <summary>Runs <paramref name="work"/> with this circuit's culture in effect.</summary>
    public void Run(Action work)
    {
        ArgumentNullException.ThrowIfNull(work);

        CultureInfo culture = CultureInfo.CurrentCulture;
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        Apply();
        try
        {
            work();
        }
        finally
        {
            Restore(culture, uiCulture);
        }
    }

    /// <inheritdoc cref="Run(Action)"/>
    public async Task RunAsync(Func<Task> work)
    {
        ArgumentNullException.ThrowIfNull(work);

        CultureInfo culture = CultureInfo.CurrentCulture;
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        Apply();
        try
        {
            // No ConfigureAwait(false): the work item was dispatched onto the renderer and its continuation
            // must stay there, so the synchronization context has to be captured.
            await work();
        }
        finally
        {
            Restore(culture, uiCulture);
        }
    }

    private void Apply()
    {
        CultureInfo.CurrentCulture = Culture;
        CultureInfo.CurrentUICulture = UiCulture;
    }

    private static void Restore(CultureInfo culture, CultureInfo uiCulture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
    }
}

