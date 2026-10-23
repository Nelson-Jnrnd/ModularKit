using Microsoft.AspNetCore.Components;

namespace ModularKit.Configuration.Blazor.Infrastructure;

/// <summary>
/// Base class for components that re-render in response to a machine, timer or domain event: it dispatches
/// those callbacks with the circuit's own culture in effect, so the render reads the same resources and the
/// same number formats as a render triggered by the operator.
/// </summary>
/// <remarks>
/// <see cref="InvokeAsync(Action)"/> and <see cref="InvokeAsync(Func{Task})"/> deliberately hide the
/// <see cref="ComponentBase"/> members of the same names. Every existing <c>InvokeAsync(StateHasChanged)</c>
/// therefore becomes culture-correct by deriving from this type, and a handler added later cannot reintroduce
/// the defect by forgetting to call something else. Work already running on the circuit — an event handler
/// bound in markup, a lifecycle method — needs nothing: it carries the culture already.
/// </remarks>
public abstract class CircuitCultureComponentBase : ComponentBase
{
    [Inject]
    private CircuitCulture CircuitCulture { get; set; } = default!;

    protected new Task InvokeAsync(Action workItem) =>
        base.InvokeAsync(() => CircuitCulture.Run(workItem));

    protected new Task InvokeAsync(Func<Task> workItem) =>
        base.InvokeAsync(() => CircuitCulture.RunAsync(workItem));
}

