# Blazor UI

Screens from the [DeviceWorkbench sample](../samples/DeviceWorkbench/README.md). The host provides the page layout and save/reload actions; ModularKit provides the configuration components.

## Browse and edit

Select an object in the tree to edit fields generated from its C# model.

![Tree navigation and device settings](images/blazor-editor.png)

## Choose an implementation

Pick a component implementation supplied by the host's type catalog.

![TCP sensor selected in the implementation picker](images/blazor-plugin-picker.png)

## Link an existing object

Names, types and paths distinguish available reference targets.

![Reference picker with two devices](images/blazor-reference-picker.png)

## Validate input

Model attributes provide field validation; this port value is outside the allowed range.

![Numeric range validation beside the field](images/blazor-validation.png)

<details>
<summary>Mobile layout</summary>

Navigation stacks above the settings form on narrow screens.

![Mobile navigation and settings](images/blazor-mobile.png)

</details>

See the [sample guide](../samples/DeviceWorkbench/README.md) to run it, or the [editor guide](editor.md) to integrate the components.
