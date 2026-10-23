namespace ModularKit.Configuration.View;

/// <summary>
/// Marks a property to be excluded from the auto-generated config form layout
/// (DynamicConfigForm grid). The property is still serialized and validated normally.
/// </summary>
/// <remarks>
/// SCOPE: This attribute only suppresses auto-rendering. It does NOT provide a
/// generic mechanism to render the property in a custom location. The framework
/// (e.g. DynamicConfigForm) decides if and where the hidden property should
/// appear via dedicated code — see <see cref="ModularKit.Configuration.ConfigEntity.Name"/>
/// which is rendered above the parameters grid by DynamicConfigForm.razor.
///
/// Plugins MAY use this attribute to suppress a property from the grid, but
/// cannot place it elsewhere without modifying the framework.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ConfigFormHiddenAttribute : Attribute { }

