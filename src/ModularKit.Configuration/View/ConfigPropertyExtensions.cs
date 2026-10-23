using ModularKit.Configuration;
using System.Reflection;

namespace ModularKit.Configuration.View;

public static class ConfigPropertyExtensions
{
    public static bool IsRequiredAssociation(this PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        if (property.GetCustomAttribute<ComponentAssociationAttribute>() is null)
            return false;

        if (!typeof(ConfigEntity).IsAssignableFrom(property.PropertyType))
            return false;

        NullabilityInfo nullability = new NullabilityInfoContext().Create(property);
        return nullability.ReadState is NullabilityState.NotNull
            || nullability.WriteState is NullabilityState.NotNull;
    }
}

