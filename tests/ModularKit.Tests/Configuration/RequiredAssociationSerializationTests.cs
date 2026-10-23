using ModularKit.Configuration.View;
using ModularKit.Configuration;
using ModularKit.Configuration.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace ModularKit.Tests.Configuration;

public class RequiredAssociationSerializationTests
{
    [Fact]
    public void Deserialize_throws_when_required_association_is_missing()
    {
        JsonSerializerOptions options = TopicJsonSerializerOptionsFactory.Create(SerializationTopics.Config);
        const string json = """
        {
          "Name": "Owner"
        }
        """;

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<AssociationOwner>(json, options));
    }

    [Fact]
    public void Required_association_null_fails_during_deserialization_or_wireup()
    {
        JsonSerializerOptions options = TopicJsonSerializerOptionsFactory.Create(SerializationTopics.Config);
        const string json = """
        {
          "Name": "Owner",
          "RequiredChild": null
        }
        """;

        Exception? deserializeException = Record.Exception(() => JsonSerializer.Deserialize<AssociationOwner>(json, options));
        if (deserializeException is not null)
        {
            Assert.IsType<JsonException>(deserializeException);
            return;
        }

        AssociationOwner owner = JsonSerializer.Deserialize<AssociationOwner>(json, options)!;
        owner.SetLoggerFactory(NullLoggerFactory.Instance);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => owner.WireUp(null));
        Assert.Contains("Required association", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Optional_association_can_be_omitted_or_null()
    {
        JsonSerializerOptions options = TopicJsonSerializerOptionsFactory.Create(SerializationTopics.Config);

        AssociationOwner omittedOwner = JsonSerializer.Deserialize<AssociationOwner>(
            """
            {
              "Name": "Owner",
              "RequiredChild": {
                "Name": "Child"
              }
            }
            """,
            options)!;
        omittedOwner.SetLoggerFactory(NullLoggerFactory.Instance);
        omittedOwner.WireUp(null);
        Assert.Null(omittedOwner.OptionalChild);

        AssociationOwner nullOwner = JsonSerializer.Deserialize<AssociationOwner>(
            """
            {
              "Name": "Owner",
              "RequiredChild": {
                "Name": "Child"
              },
              "OptionalChild": null
            }
            """,
            options)!;
        nullOwner.SetLoggerFactory(NullLoggerFactory.Instance);
        nullOwner.WireUp(null);
        Assert.Null(nullOwner.OptionalChild);
    }

    [SerializationTopic(SerializationTopics.Config)]
    private sealed class AssociationOwner : ConfigEntity
    {
        [SerializationTopic(SerializationTopics.Config)]
        [ComponentAssociation("Required child", AssociationType.Composition)]
        public AssociationChild RequiredChild { get; set; } = null!;

        [SerializationTopic(SerializationTopics.Config)]
        [ComponentAssociation("Optional child", AssociationType.Composition)]
        public AssociationChild? OptionalChild { get; set; }
    }

    [SerializationTopic(SerializationTopics.Config)]
    private sealed class AssociationChild : ConfigEntity
    {
    }
}

