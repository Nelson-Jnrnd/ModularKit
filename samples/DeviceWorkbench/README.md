# Device workbench

A local example combining all four libraries. See the [UI tour](../../docs/blazor-ui.md) for screenshots.

## Run

From the repository root with a .NET 10 SDK:

```sh
dotnet run --project samples/DeviceWorkbench --urls http://localhost:5080
```

Open `http://localhost:5080`.

1. Select **Devices**, add a **TCP sensor**, and set its host and port.
2. Add another sensor to try duplicate host/port validation.
3. Select the root and choose a **Selected device** reference.
4. Save and reload; rename the referenced device and repeat to check path updates.
5. Remove the reference before deleting that device.

## Projects

| Project | Purpose |
| --- | --- |
| DeviceWorkbench.Contracts | Shared model and plugin information contract |
| DeviceWorkbench.Plugin | TCP sensor implementation and service registration |
| DeviceWorkbench | Blazor host, type-catalog adapter and persistence |

Build and publish stage the plugin DLL under `plugins/`; the host loads it at runtime without a compile-time assembly reference. The [theme and icon guide](../../docs/editor.md#theme-and-icons) explains appearance customization.

## Storage

Configuration is saved to `storage/workbench.json` under the host's content root, with `.bak` on replacement. Storage is ignored by Git. Read errors are displayed rather than replaced with defaults.

The sample connects to no hardware and is intended for one local user. Sessions edit separate models but save to the same file; concurrent use needs host coordination.
