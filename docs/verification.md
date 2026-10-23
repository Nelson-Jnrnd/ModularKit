# Extraction verification

Verified locally on Linux with .NET SDK 10.0.402.

| Check | Result |
| --- | --- |
| Release build of libraries, sample and tests | Passed; no warnings or errors |
| Automated tests | 28 passed, none failed or skipped |
| NuGet packaging | Four independent preview packages produced, each with its README |
| External package consumer | Restored all four local packages, compiled, saved/reloaded a configuration and created an editor tree node |
| Sample host | Started and loaded the separately staged sensor plugin |
| Sample HTTP response and static assets | Homepage, MudBlazor CSS/JavaScript and Blazor JavaScript returned HTTP 200 |
| Markdown links and extraction destinations | Passed `scripts/check_docs.py` |
| Mermaid syntax | All five diagrams parsed with Mermaid 11 |

The tests include references and required associations, configuration/state filtering, plugin identifiers and aliases, unknown-type rejection, duplicate registrations, scoped constructor dependencies, JSON replacement/backups/cancellation, editor rendering and field editing, and deletion of referenced objects.

The CI workflow runs build, tests, documentation link checks and packaging on Linux and Windows. These are configured checks; local Linux results alone do not establish the Windows outcome.

Browser-driven interaction and accessibility testing have not been completed. Automated component rendering, input editing and host HTTP checks were completed. The environment's browser download failed, so this report does not claim a full browser end-to-end pass.

Softponics has not been migrated to these packages. Its original test suite and real installation files were not used as proof of application compatibility. Follow the [migration guide](softponics-migration.md) before that integration.
