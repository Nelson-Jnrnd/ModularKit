using System.Text.Json;

namespace ModularKit.Configuration.Serialization;

/// <summary>
/// Provides configured <see cref="JsonSerializerOptions"/> instances
/// used by the application for different serialization topics
/// </summary>
public interface ISerializationOptionsProvider
{
    /// <summary>
    /// Gets the base <see cref="JsonSerializerOptions"/> instance that contains
    /// shared configuration (converters, polymorphism, etc.) before topic filtering.
    /// </summary>
    JsonSerializerOptions Base { get; }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> configured for serializing
    /// and deserializing configuration data (properties marked with the Config topic).
    /// </summary>
    JsonSerializerOptions Config { get; }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> configured for serializing
    /// and deserializing runtime state data (properties marked with the State topic).
    /// </summary>
    JsonSerializerOptions State { get; }
}

