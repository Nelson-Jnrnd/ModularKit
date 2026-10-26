# Repository instructions

Read README.md and docs/architecture.md.

- Keep Plugins, Configuration and Persistence.Json independent. Blazor references Configuration; adapters belong in hosts.
- Keep application models out of generic libraries. Preserve saved identifiers, member names and reference paths.
- Document compatibility changes in library guides. Keep explanations concise, with Mermaid UML and actual UI screenshots.
- Cover behaviour changes with regression tests; check editor changes in DeviceWorkbench.

```sh
dotnet build ModularKit.slnx -c Release
dotnet test ModularKit.slnx -c Release --no-build
dotnet pack ModularKit.slnx -c Release --no-build -o artifacts/packages
python scripts/check_docs.py
```

Publish packages or change consumer installations only when requested.
