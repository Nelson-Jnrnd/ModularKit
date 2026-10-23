# Contributing

Use the .NET 10 SDK selected by `global.json`. No hardware, database or Softponics checkout is required.

```sh
dotnet restore ModularKit.slnx
dotnet build ModularKit.slnx -c Release --no-restore
dotnet test ModularKit.slnx -c Release --no-build
dotnet pack ModularKit.slnx -c Release --no-build -o artifacts/packages
python scripts/check_docs.py
```

## Boundaries

- Plugins, Configuration and Persistence.Json must not reference each other.
- Configuration.Blazor may reference Configuration and MudBlazor, but not Plugins or application models.
- Keep integration adapters in hosts or samples.
- Keep saved type IDs, member names and configuration paths stable. Document breaking changes and add a compatibility fixture when changing them.
- Keep explanations direct and examples executable. Use fenced Mermaid diagrams for relationships and UML; avoid screenshots of diagrams.
- Do not commit generated outputs, local storage, credentials or installation files.

## Validation

The tests exercise plugin assembly loading and service configuration, aliases and unknown types, reference integrity and renaming, topic filtering, persistence replacement/cancellation and editor rendering. The sample builds an independently staged plugin, demonstrating that the libraries do not require Softponics.

For an editor change, run the sample and check add, edit, required fields, duplicate host/port combinations, reference selection, deletion restrictions, save and reload. Automated rendering checks do not replace a full browser accessibility review.

## Packages and releases

All packages initially share a preview version in `Directory.Build.props`. CI builds package artifacts but does not publish or tag releases. Consumers choose individual packages. A future release should record supported runtime/MudBlazor versions, API changes and migration instructions.
