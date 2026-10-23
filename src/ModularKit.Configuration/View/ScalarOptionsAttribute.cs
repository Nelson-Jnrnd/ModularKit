namespace ModularKit.Configuration.View;

/// <summary>
/// Marks a scalar property whose value must be picked from a list supplied at runtime rather than
/// typed freely. The configuration form renders a selection list instead of a text field and keeps
/// storing the option value.
/// </summary>
/// <param name="providerType">
/// The <see cref="IScalarOptionsProvider"/> implementation that supplies the options. It must be
/// registered under <see cref="IScalarOptionsProvider"/> for the form to find it.
/// </param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ScalarOptionsAttribute(Type providerType) : Attribute
{
    public Type ProviderType { get; } = providerType;
}

