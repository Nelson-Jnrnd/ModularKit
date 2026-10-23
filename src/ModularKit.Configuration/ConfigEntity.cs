using ModularKit.Configuration.References;
using ModularKit.Configuration.View;
using ModularKit.Configuration.Serialization;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ModularKit.Configuration;

[SerializationTopic(SerializationTopics.Config)]
[SerializationTopic(SerializationTopics.State)]
public abstract class ConfigEntity
{
    // Promoted to public to allow UI validation (DynamicConfigForm.CreateValidationFor)
    // to surface the same rule the setter enforces, without duplicating the character set.
    public static readonly char[] InvalidNameCharacters = ['/', ',', '|'];

    private string _name = "Untitled";
    private bool _isWired;
    private ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Sets the <see cref="ILoggerFactory"/> for this context. Call this on the root entity before <see cref="WireUp"/>.
    /// </summary>
    public void SetLoggerFactory(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

    /// <summary>
    /// Returns the <see cref="ILoggerFactory"/> for this context by traversing up to the root where it was set.
    /// Throws if <see cref="SetLoggerFactory"/> was not called on the root before wiring.
    /// </summary>
    protected ILoggerFactory LoggerFactory =>
        _loggerFactory
        ?? Parent?.LoggerFactory
        ?? throw new InvalidOperationException(
            $"ILoggerFactory is not set. Call SetLoggerFactory on the root before WireUp. Entity: {GetType().Name} '{Name}'");

    [JsonInclude]
    [SerializationTopic(SerializationTopics.Config)]
    [SerializationTopic(SerializationTopics.State)]
    [Required(ErrorMessage = "Name cannot be empty.")]
    [Unique(Message = "This name is already used.")]
    [ConfigFormHidden]
    public string Name {
        get => _name;
        set {
            if (value.IndexOfAny(InvalidNameCharacters) >= 0)
                throw new ArgumentException($"Entity name must not contain any of the characters: {string.Join(" ", InvalidNameCharacters)}.");

            // Sibling-name uniqueness is enforced here, not only through the [Unique] UI validation:
            // duplicate sibling names produce identical Paths and corrupt path-keyed references.
            if (Siblings.Any(sibling => string.Equals(sibling.Name, value, StringComparison.Ordinal)))
                throw new ArgumentException($"An entity with the name '{value}' already exists among siblings.");

            if (string.Equals(_name, value, StringComparison.Ordinal))
                return;

            // Before the tree is wired (construction / deserialization / LoadFrom), Path is not
            // yet meaningful and no references point into this subtree, so just set the field.
            if (!_isWired)
            {
                _name = value;
                return;
            }

            // On a wired tree, renaming changes the Path of this entity and all descendants.
            // References are keyed by Path, so rewrite the stored paths of every reference that
            // targets this subtree to keep them resolvable.
            string oldPath = Path;
            _name = value;
            RepairReferencePathsAfterRename(oldPath, Path);
        }
    }

    /// <summary>
    /// Assigns a name unique among the children of <paramref name="owner"/>, formed from
    /// <paramref name="baseName"/> and the smallest free ordinal starting at 1.
    /// </summary>
    /// <remarks>
    /// Meant for a freshly created entity that is not attached yet: the probe reads the owner's
    /// current children, which do not contain this entity, so the entity's own name never blocks
    /// the ordinal it is about to take. Characters rejected by <see cref="Name"/> are dropped from
    /// <paramref name="baseName"/>; if nothing usable remains, the runtime type name is used.
    /// </remarks>
    public void AssignGeneratedNameUnder(ConfigEntity owner, string? baseName)
    {
        ArgumentNullException.ThrowIfNull(owner);

        string prefix = BuildNamePrefix(baseName);
        HashSet<string> takenNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (ConfigEntity child in owner.AllChildren)
        {
            if (!ReferenceEquals(child, this))
                takenNames.Add(child.Name);
        }

        int ordinal = 1;
        while (takenNames.Contains($"{prefix} {ordinal}"))
            ordinal++;

        Name = $"{prefix} {ordinal}";
    }

    private string BuildNamePrefix(string? baseName)
    {
        string prefix = string.Concat((baseName ?? string.Empty)
            .Where(character => Array.IndexOf(InvalidNameCharacters, character) < 0))
            .Trim();

        return prefix.Length == 0 ? GetType().Name : prefix;
    }

    private void RepairReferencePathsAfterRename(string oldPath, string newPath)
    {
        foreach (IConfigRef reference in Root.GetAllConfigRefs())
        {
            if (reference.Path is null)
                continue;

            if (string.Equals(reference.Path, oldPath, StringComparison.Ordinal))
                reference.Path = newPath;
            else if (reference.Path.StartsWith(oldPath + "/", StringComparison.Ordinal))
                reference.Path = newPath + reference.Path[oldPath.Length..];
        }
    }

    [JsonIgnore]
    public ConfigEntity? Parent { get; private set; }

    public T GetAncestor<T>() where T : ConfigEntity
    {
        if (TryGetAncestor<T>(out T ancestor))
            return ancestor;

        throw new InvalidOperationException(
            $"{GetType().Name} '{Name}' has no ancestor of type '{typeof(T).Name}'.");
    }

    public bool TryGetAncestor<T>(out T ancestor) where T : ConfigEntity
    {
        for (ConfigEntity? current = Parent; current is not null; current = current.Parent)
        {
            if (current is T match)
            {
                ancestor = match;
                return true;
            }
        }

        ancestor = null!;
        return false;
    }

    public string Path => Parent is null ? _name : $"{Parent.Path}/{Name}";
    public override string ToString() => Path;

    public void WireUp(ConfigEntity? owner)
    {
        AttachToTree(owner);
        ResolveReferences();
        ValidateRequiredAssociations();
        NotifyReferencesResolved();
    }

    public IReadOnlyList<string> ClearBrokenConfigReferences()
    {
        EnsureTreeIsWired();

        List<string> brokenReferences = [];
        foreach (ConfigReferenceEntry entry in EnumerateConfigReferences())
        {
            if (entry.Reference is IRequiredConfigRef)
                continue;

            if (entry.Reference.Path is null || entry.Reference.Value is not null)
                continue;

            brokenReferences.Add(FormatReferenceError(entry));
            ClearReference(entry.Reference);
        }

        return brokenReferences;
    }

    public void PrepareConfigReferencesForPersistence()
    {
        EnsureTreeIsWired();

        List<ConfigReferenceEntry> brokenReferences = [];
        foreach (ConfigReferenceEntry entry in EnumerateConfigReferences())
        {
            if (entry.Reference.Value is not null)
            {
                SyncPathFromComponent(entry.Reference);
                continue;
            }

            if (entry.Reference is IRequiredConfigRef || entry.Reference.Path is not null)
                brokenReferences.Add(entry);
        }

        if (brokenReferences.Count > 0)
            throw new ConfigIntegrityException(brokenReferences);
    }

    private void ResolveReferences()
    {
        // Resolve references in the current entity
        foreach (object value in ConfigPropertyValues)
        {
            if (value is IConfigRef singleRef)
            {
                singleRef.Resolve(this);
                continue;
            }
            if (value is IEnumerable enumerable && value is not string)
            {
                foreach (object? item in enumerable)
                    if (item is IConfigRef itemRef)
                        itemRef.Resolve(this);
            }
        }
        // Recursively resolve references in child entities
        foreach (ConfigEntity child in AllChildren)
            child.ResolveReferences();
    }

    private void NotifyReferencesResolved()
    {
        OnReferencesResolved();
        foreach (ConfigEntity child in AllChildren)
            child.NotifyReferencesResolved();
    }

    protected virtual void OnReferencesResolved() { }

    private void ValidateRequiredAssociations()
    {
        foreach (PropertyInfo property in ConfigProperties)
        {
            if (!property.IsRequiredAssociation())
                continue;

            object? value = property.GetValue(this);
            if (value is null)
            {
                throw new InvalidOperationException(
                    $"Required association '{GetType().Name}.{property.Name}' is missing.");
            }
        }

        foreach (ConfigEntity child in AllChildren)
            child.ValidateRequiredAssociations();
    }

    protected virtual void AttachToTree(ConfigEntity? owner)
    {
        _isWired = true;
        Parent = owner;
        foreach (ConfigEntity child in AllChildren)
        {
            child.AttachToTree(this);
        }
    }

    public IEnumerable<ConfigEntity> AllDescendants
    {
        get
        {
            foreach (ConfigEntity child in AllChildren)
            {
                yield return child;
                foreach (ConfigEntity descendant in child.AllDescendants)
                {
                    yield return descendant;
                }
            }
        }
    }

    public IEnumerable<ConfigEntity> AllChildren
    {
        get
        {
            foreach (object value in ConfigPropertyValues)
            {
                if (value is ConfigEntity entity)
                {
                    yield return entity;
                    continue;
                }

                if (value is IEnumerable enumerable && value is not string)
                {
                    foreach (object? item in enumerable)
                    {
                        if (item is ConfigEntity child)
                            yield return child;
                    }
                }
            }
        }
    }

    public ConfigEntity Root => Parent?.Root ?? this;

    /// <summary>
    /// Direct siblings of this entity (children of the same <see cref="Parent"/>), excluding itself.
    /// </summary>
    public ConfigEntity[] Siblings =>
        Parent is null
            ? []
            : [.. Parent.AllChildren.Where(e => !ReferenceEquals(e, this))];

    internal IEnumerable<IConfigRef> GetAllConfigRefs()
    {
        foreach (object value in ConfigPropertyValues)
        {
            if (value is IConfigRef r) { yield return r; continue; }
            if (value is IEnumerable e && value is not string)
                foreach (object? item in e)
                    if (item is IConfigRef itemRef) yield return itemRef;
        }
        foreach (ConfigEntity child in AllChildren)
            foreach (IConfigRef r in child.GetAllConfigRefs())
                yield return r;
    }

    public IReadOnlyList<ConfigReferenceEntry> GetIncomingConfigReferences()
    {
        EnsureTreeIsWired();

        List<ConfigReferenceEntry> descriptions = [];
        ConfigEntity root = Root;

        foreach (ConfigReferenceEntry entry in root.EnumerateConfigReferences())
        {
            if (!string.Equals(entry.Reference.Path, Path, StringComparison.Ordinal))
                continue;

            if (string.Equals(entry.OwnerPath, Path, StringComparison.Ordinal))
                continue;

            descriptions.Add(entry);
        }

        return descriptions;
    }

    protected IReadOnlyList<T> FindRelatedEntities<T>() where T : ConfigEntity
    {
        IEnumerable<T> containedIn = Root.AllDescendants.OfType<T>()
            .Where(e => Path.StartsWith(e.Path + "/"));
        IEnumerable<T> referencedBy = Root.AllDescendants.OfType<T>()
            .Where(e => e.GetAllConfigRefs().Any(r => r.Path == Path));
        return [.. containedIn.Concat(referencedBy).Distinct()];
    }

    internal IEnumerable<object> ConfigPropertyValues
    {
        get
        {
            foreach (PropertyInfo prop in ConfigProperties)
            {
                object? value;
                try
                {
                    value = prop.GetValue(this);
                }
                catch
                {
                    continue;
                }

                if (value is null)
                    continue;

                yield return value;
            }
        }
    }

    private void EnsureTreeIsWired()
    {
        if (!_isWired)
        {
            throw new InvalidOperationException(
                $"Config reference integrity checks require a wired tree. Entity '{GetType().Name}' with name '{Name}' is not wired.");
        }

        foreach (ConfigEntity child in AllChildren)
            child.EnsureTreeIsWired();
    }

    private IEnumerable<ConfigReferenceEntry> EnumerateConfigReferences()
    {
        foreach (PropertyInfo property in ConfigProperties)
        {
            object? value;
            try
            {
                value = property.GetValue(this);
            }
            catch
            {
                continue;
            }

            if (value is null)
                continue;

            if (value is IConfigRef singleRef)
            {
                yield return CreateReferenceEntry(property.Name, singleRef);
                continue;
            }

            if (value is IEnumerable enumerable && value is not string)
            {
                foreach (object? item in enumerable)
                {
                    if (item is IConfigRef itemRef)
                        yield return CreateReferenceEntry(property.Name, itemRef);
                }
            }
        }

        foreach (ConfigEntity child in AllChildren)
        {
            foreach (ConfigReferenceEntry entry in child.EnumerateConfigReferences())
                yield return entry;
        }
    }

    private static void ClearReference(IConfigRef reference)
    {
        try
        {
            reference.Value = null;
        }
        catch (NotImplementedException)
        {
            throw new InvalidOperationException(
                $"Unsupported config reference implementation '{reference.GetType().FullName}' encountered during integrity cleanup.");
        }
    }

    private static void SyncPathFromComponent(IConfigRef reference)
    {
        try
        {
            reference.SyncPath();
        }
        catch (NotImplementedException)
        {
            throw new InvalidOperationException(
                $"Unsupported config reference implementation '{reference.GetType().FullName}' encountered during persistence preparation.");
        }
    }

    private IEnumerable<PropertyInfo> ConfigProperties
    {
        get
        {
            Type type = GetType();

            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!Attribute.IsDefined(prop, typeof(SerializationTopicAttribute)))
                    continue;

                if (!prop.CanRead || prop.GetIndexParameters().Length != 0)
                    continue;

                if (Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
                    continue;

                yield return prop;
            }
        }
    }

    private static string FormatReferenceError(ConfigReferenceEntry entry)
    {
        return $"- Owner '{entry.OwnerPath}' property '{entry.PropertyName}' -> '{entry.TargetPath}' ({entry.Reference.ValueType.Name})";
    }

    private ConfigReferenceEntry CreateReferenceEntry(string propertyName, IConfigRef reference)
    {
        string ownerPath = Parent is null ? Name : Path;
        return new ConfigReferenceEntry(ownerPath, propertyName, reference.Path, reference);
    }
}

