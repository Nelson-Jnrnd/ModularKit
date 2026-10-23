# Repository instructions

Read README.md, docs/architecture.md and CONTRIBUTING.md before implementation.

Maintain the package boundaries documented there. Generic libraries must not import application-specific models or storage configuration. Preserve the extraction source record and explain intentional differences in docs/softponics-migration.md.

Use the build/test/pack commands in CONTRIBUTING.md. Validate generated forms against the DeviceWorkbench sample and add meaningful regression coverage for changed persistence or reference behaviour. Documentation uses plain explanations and Mermaid UML diagrams that GitHub can render.

Do not publish packages, introduce a license grant, or change Softponics installations as part of routine library work. Repository changes requested by the user can be committed and proposed normally.
