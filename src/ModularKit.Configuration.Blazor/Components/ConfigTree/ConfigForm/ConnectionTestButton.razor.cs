using ModularKit.Configuration;
using Microsoft.AspNetCore.Components;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;

public partial class ConnectionTestButton
{
    [Parameter, EditorRequired]
    public required IConnectionTestable Target { get; set; }

    private bool _testing;
    private ConnectionTestResult? _result;

    private async Task TestAsync()
    {
        _testing = true;
        _result = null;
        StateHasChanged();
        // Yield once so the disabled button + "Testing..." label render before the (potentially
        // long-running) test starts. Without this, a synchronous-blocking call inside
        // TestConnectionAsync would freeze the circuit before the UI can update.
        await Task.Yield();
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            _result = await Target.TestConnectionAsync(cts.Token);
        }
        catch (NotSupportedException ex)
        {
            _result = new ConnectionTestResult(false, null, null, TimeSpan.Zero, ex.Message);
        }
        catch (OperationCanceledException)
        {
            _result = new ConnectionTestResult(false, null, null, TimeSpan.Zero, "Timeout (10 s)");
        }
        catch (Exception ex)
        {
            _result = new ConnectionTestResult(false, null, null, TimeSpan.Zero, ex.Message);
        }
        finally
        {
            _testing = false;
        }
    }
}

