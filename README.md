# ModularKit

.NET libraries for loading plugins, configuring connected objects and saving JSON. Targets **.NET 10**; the optional editor uses **Blazor Server and MudBlazor 9**.

## Libraries

| Library | Provides | ModularKit dependencies |
| --- | --- | --- |
| [ModularKit.Plugins](docs/plugins.md) | Plugin loading, service registration and polymorphic JSON | None |
| [ModularKit.Configuration](docs/configuration.md) | Configuration trees, references and serialization metadata | None |
| [ModularKit.Configuration.Blazor](docs/editor.md) | Generated forms, tree navigation and selection dialogs | Configuration |
| [ModularKit.Persistence.Json](docs/persistence.md) | JSON file storage with replacement and backups | None |

Use each library independently or combine them in your application. Packages are currently a preview and have not been published to NuGet.org; use project references or build local packages with `dotnet pack ModularKit.slnx -c Release -o artifacts/packages`.

## Blazor UI

Browse objects, edit generated fields, choose component implementations and link existing objects.

![Configuration tree and generated settings form](docs/images/blazor-editor.png)

[View the UI tour](docs/blazor-ui.md), including dialogs, validation and mobile layout.

## Run the example

With a .NET 10 SDK installed, run from the repository root:

```sh
dotnet run --project samples/DeviceWorkbench --urls http://localhost:5080
```

Open `http://localhost:5080`. The [sample guide](samples/DeviceWorkbench/README.md) covers the workflow, project layout and storage.

## Documentation

- [Architecture and UML](docs/architecture.md)
- [Plugin setup and type identifiers](docs/plugins.md)
- [Configuration models and references](docs/configuration.md)
- [Blazor integration, theme and icons](docs/editor.md)
- [JSON persistence](docs/persistence.md)

Plugins are trusted code loaded at startup; unloading and dependency-version isolation are not supported. The sample is intended for one local user.
