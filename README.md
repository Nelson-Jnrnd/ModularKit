# ModularKit

Reusable .NET libraries for applications that load plugins and let users configure connected objects.

Extracted from Softponics and separated from its greenhouse, machine-control, identity and database code. Targets **.NET 10**; the editor uses **Blazor Server and MudBlazor 9**. This is an initial preview, with independently packable libraries. Packages have not been published to NuGet.org.

## Libraries

| Library | Provides | Other ModularKit dependencies |
| --- | --- | --- |
| [ModularKit.Plugins](docs/plugins.md) | Startup assembly loading, service registration, implementation selection, polymorphic JSON | None |
| [ModularKit.Configuration](docs/configuration.md) | Configuration tree, object references, validation metadata, configuration/state JSON selection | None |
| [ModularKit.Configuration.Blazor](docs/editor.md) | Tree navigation, generated forms, creation and selection dialogs | Configuration |
| [ModularKit.Persistence.Json](docs/persistence.md) | JSON file reads, temporary writes, replacement and backup | None |

Install only what your application uses. Configuration and persistence work without plugins or a web interface.

## Try it

Install a .NET 10 SDK, then run from the repository root:

```sh
dotnet restore ModularKit.slnx
dotnet build ModularKit.slnx -c Release --no-restore
dotnet test ModularKit.slnx -c Release --no-build
dotnet run --project samples/DeviceWorkbench --urls http://localhost:5080
```

Open `http://localhost:5080`. Select **Devices**, add a **TCP sensor**, edit its host and port, select it through **Selected device**, then save and reload. The sensor implementation comes from a separate DLL staged into the application's `plugins` directory. Nothing connects to real hardware.

The [sample guide](samples/DeviceWorkbench/README.md) explains the host, plugin, shared contracts and storage location.

## Use in another solution

For source-based development:

```sh
dotnet add YourApp/YourApp.csproj reference path/to/ModularKit/src/ModularKit.Plugins/ModularKit.Plugins.csproj
```

Or build local packages:

```sh
dotnet pack ModularKit.slnx -c Release -o artifacts/packages
dotnet add YourApp/YourApp.csproj package ModularKit.Plugins --version 0.1.0-preview.1 --source path/to/ModularKit/artifacts/packages
```

Your normal NuGet source must also be configured to restore third-party dependencies. Package IDs and APIs are preview contracts and may change before 1.0.

## Documentation

- [Getting started](docs/getting-started.md): the smallest useful examples.
- [Architecture](docs/architecture.md): package boundaries and Mermaid UML diagrams.
- [Plugins](docs/plugins.md): loading, registration, type identifiers and deployment.
- [Configuration](docs/configuration.md): model attributes, references and lifecycle.
- [Blazor editor](docs/editor.md): host setup, edit permissions and validation.
- [JSON persistence](docs/persistence.md): writes, backups, cancellation and limits.
- [Softponics migration](docs/softponics-migration.md): compatibility rules and remaining integration work.
- [Extraction provenance](docs/provenance.md): source revision and adaptation record.
- [Verification](docs/verification.md): checks performed and remaining validation.
- [Contributing](CONTRIBUTING.md): development and validation commands.

## Scope

Plugin assemblies are trusted application code loaded at startup. There is no sandbox, hot reload or dependency-version isolation. The sample is a local, single-user demonstration; it does not implement authentication or concurrent editing. Existing Softponics installations continue to use their original code until a separate migration is validated.
