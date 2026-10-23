# Extraction provenance

Source: `Nelson-Jnrnd/Softponics`, commit `601a682ab3a7e540469da6fd7823d5679e4bc8f3`.

The [machine-readable manifest](extraction-manifest.json) maps adapted source files to their ModularKit destinations. The source repository is private; access to it is not necessary to build ModularKit.

## Adaptations

- Replaced the original namespaces with ModularKit package namespaces.
- Kept the existing configuration entities, references, view metadata, generated forms, tree and selection components as the extraction baseline.
- Replaced the domain-specific plugin manager registration with host-declared base types and known assemblies.
- Replaced plugin references in the editor with configuration-owned `IConfigTypeCatalog` and `ConfigTypeOption`.
- Separated serialization options from the plugin manager; hosts supply base JSON options.
- Exposed the configuration operations needed across assembly boundaries.
- Applied deletion checks to both editor deletion routes, including references into removed subtrees, and encoded user-entered names in deletion messages.
- Added stable type IDs, explicit read aliases and duplicate-registration checks.
- Kept JSON persistence independent of all other packages.
- Retained only editor resource entries from the original UI resources.
- Ported generic required-reference, required-association and selected-node regression tests, and added independent device, persistence, plugin and editor tests.

The example application, package layout, host adapters, CI and documentation were added for ModularKit. Greenhouse data, operational configuration, account storage, secrets and hardware implementations are excluded. No license grant has been added as part of this extraction; repository visibility alone is not an open-source license.
