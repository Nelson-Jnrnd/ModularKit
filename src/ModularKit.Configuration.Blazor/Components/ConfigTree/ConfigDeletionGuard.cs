using ModularKit.Configuration.References;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree;

internal static class ConfigDeletionGuard
{
    internal static IReadOnlyList<ConfigReferenceEntry> GetExternalReferences(ConfigEntity entity)
    {
        ConfigEntity[] removed = [entity, .. entity.AllDescendants];
        HashSet<string> paths = new(removed.Select(item => item.Path), StringComparer.Ordinal);
        return [.. removed.SelectMany(item => item.GetIncomingConfigReferences())
            .Where(reference => !paths.Contains(reference.OwnerPath))];
    }
}
