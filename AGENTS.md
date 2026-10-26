# Repository instructions

Read README.md and docs/architecture.md before implementation.

Maintain the package boundaries documented there. Generic libraries must not import application-specific models or storage configuration. Document compatibility changes in docs/compatibility.md.

Use the build/test/pack commands in README.md and run `python scripts/check_docs.py`. Validate generated forms against the DeviceWorkbench sample and add meaningful regression coverage for changed persistence or reference behaviour. Documentation uses plain explanations and Mermaid UML diagrams that GitHub can render.

Do not publish packages, introduce a license grant, or change consumer installations as part of routine library work. Repository changes requested by the user can be committed and proposed normally.
