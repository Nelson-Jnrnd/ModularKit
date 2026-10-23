using ModularKit.Configuration.References;
using ModularKit.Configuration;
using ModularKit.Configuration.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ModularKit.Tests.Configuration;

public class ConfigRefRequirednessTests
{
    [Fact]
    public void ClearBrokenConfigReferences_clears_optional_references_only()
    {
        ReferencedEntity target = new() { Name = "Target" };
        RefOwner owner = new()
        {
            Name = "Owner",
            OptionalTargetRef = new ConfigRef<ReferencedEntity> { Path = "Root/MissingOptional" },
            RequiredTargetRef = new RequiredConfigRef<ReferencedEntity> { Path = "Root/MissingRequired" }
        };
        RefRoot root = CreateWiredRoot(target, owner);

        IReadOnlyList<string> cleared = root.ClearBrokenConfigReferences();

        Assert.Single(cleared);
        Assert.Null(owner.OptionalTargetRef.Path);
        Assert.Null(owner.OptionalTargetRef.Value);
        Assert.Equal("Root/MissingRequired", owner.RequiredTargetRef.Path);
        Assert.Null(owner.RequiredTargetRef.Value);
    }

    [Fact]
    public void PrepareConfigReferencesForPersistence_rejects_missing_required_references()
    {
        ReferencedEntity target = new() { Name = "Target" };
        RefOwner owner = new()
        {
            Name = "Owner",
            OptionalTargetRef = new ConfigRef<ReferencedEntity> { Value = target }
        };
        RefRoot root = CreateWiredRoot(target, owner);
        owner.OptionalTargetRef.Value = target;

        ConfigIntegrityException ex = Assert.Throws<ConfigIntegrityException>(() => root.PrepareConfigReferencesForPersistence());

        ConfigReferenceEntry error = Assert.Single(ex.ReferenceErrors);
        Assert.Equal(nameof(RefOwner.RequiredTargetRef), error.PropertyName);
        Assert.IsAssignableFrom<IRequiredConfigRef>(error.Reference);
    }

    [Fact]
    public void RequiredConfigRef_rejects_null_assignment_after_being_set()
    {
        ReferencedEntity target = new() { Name = "Target" };
        RequiredConfigRef<ReferencedEntity> reference = new() { Value = target };

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => reference.Value = null);

        Assert.Contains("cannot be cleared", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static RefRoot CreateWiredRoot(ReferencedEntity target, RefOwner owner)
    {
        RefRoot root = new()
        {
            Name = "Root",
            Targets = [target],
            Owners = [owner]
        };

        root.SetLoggerFactory(NullLoggerFactory.Instance);
        root.WireUp(null);
        return root;
    }

    private sealed class RefRoot : ConfigEntity
    {
        [SerializationTopic(SerializationTopics.Config)]
        public List<ReferencedEntity> Targets { get; set; } = [];

        [SerializationTopic(SerializationTopics.Config)]
        public List<RefOwner> Owners { get; set; } = [];
    }

    private sealed class RefOwner : ConfigEntity
    {
        [SerializationTopic(SerializationTopics.Config)]
        public ConfigRef<ReferencedEntity> OptionalTargetRef { get; set; } = new();

        [SerializationTopic(SerializationTopics.Config)]
        public RequiredConfigRef<ReferencedEntity> RequiredTargetRef { get; set; } = new();
    }

    private sealed class ReferencedEntity : ConfigEntity
    {
    }
}

