# Device workbench

A local demonstration of the four libraries without greenhouse code or hardware access.

See the [UI tour](../../docs/blazor-ui.md) for screenshots of the sample and its configuration components.

```sh
dotnet run --project samples/DeviceWorkbench --urls http://localhost:5080
```

Run from the repository root and open `http://localhost:5080`.

1. Select **Devices** and add a **TCP sensor**.
2. Set its host and port. A second sensor using the same pair should fail editor uniqueness validation.
3. Select the workbench root and choose a **Selected device** reference.
4. Save, reload, and confirm the device values and selection persist.
5. Rename the device, save and reload again. Its reference follows the new path.
6. A referenced device cannot be deleted until its reference is removed.

## Projects

| Project | Purpose |
| --- | --- |
| DeviceWorkbench.Contracts | Shared `Workbench`, abstract `Device` and plugin information contract |
| DeviceWorkbench.Plugin | Concrete `TcpSensor`, stable type ID and plugin service registration |
| DeviceWorkbench | Blazor host, configuration/plugin adapter and save/reload integration |

The host has a build dependency on the plugin to stage its DLL, but does not reference that DLL as a compile-time assembly. It loads `plugins/DeviceWorkbench.Plugin.dll` through ModularKit. Build and publish targets both copy the DLL to the expected directory.

The host supplies its theme, responsive layout and Material icons. The device collection is edited through the navigation tree rather than duplicated in the root object's generated form. See the [theme and icon guide](../../docs/editor.md#theme-and-icons) to adapt the appearance in another application.

Configuration is saved under `samples/DeviceWorkbench/storage/workbench.json` when run from the source tree. A replacement retains `workbench.json.bak`. Storage is ignored by Git. Errors are displayed rather than silently replacing an unreadable file with defaults.

This example is intended for one local user. Each circuit edits its own in-memory model, while saves use the same file; simultaneous sessions require a host concurrency policy. It has no authentication and no network-device operations. Do not use the sample as an externally exposed administration service.
