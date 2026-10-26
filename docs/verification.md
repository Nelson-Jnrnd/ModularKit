# Verification

Verified locally on Linux with .NET SDK 10.0.402.

| Check | Result |
| --- | --- |
| Release build of libraries, sample and tests | Passed; no warnings or errors |
| Automated tests | 28 passed, none failed or skipped |
| NuGet packaging | Four independent preview packages produced, each with its README |
| External package consumer | Restored all four local packages, compiled, saved/reloaded a configuration and created an editor tree node |
| Sample host | Started and loaded the separately staged sensor plugin |
| Sample HTTP response and static assets | Homepage, MudBlazor CSS/JavaScript and Blazor JavaScript returned HTTP 200 |
| Browser smoke checks | Settings navigation, plugin and reference dialogs, save/reload, and invalid numeric input verified in Chromium |
| Markdown links | Passed `scripts/check_docs.py` |
| Mermaid syntax | All five diagrams parsed with Mermaid 11 |

The tests include references and required associations, configuration/state filtering, plugin identifiers and aliases, unknown-type rejection, duplicate registrations, scoped constructor dependencies, JSON replacement/backups/cancellation, editor rendering and field editing, and deletion of referenced objects.

The CI workflow runs build, tests, documentation link checks and packaging on Linux and Windows. These are configured checks; local Linux results alone do not establish the Windows outcome.

The [UI tour](blazor-ui.md) contains screenshots from the running sample. Browser smoke checks covered settings navigation, selecting a plugin type, selecting and persisting a reference, reloading configuration, and displaying a numeric range error. These checks do not replace a full browser compatibility or accessibility review.

Applications adopting these packages should validate their own saved files and integration behaviour. Follow the [compatibility guide](compatibility.md) when changing persisted types or paths.
