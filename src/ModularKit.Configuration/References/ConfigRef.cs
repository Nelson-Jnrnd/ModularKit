using ModularKit.Configuration;
using ModularKit.Configuration.Serialization;
using System.Text.Json.Serialization;

namespace ModularKit.Configuration.References;

[SerializationTopic(SerializationTopics.Config)]
public class ConfigRef<TValue> : ConfigRefBase<TValue>
    where TValue : ConfigEntity
{
}

[SerializationTopic(SerializationTopics.Config)]
public abstract class ConfigRefBase<TValue> : IConfigRef<TValue>
    where TValue : ConfigEntity
{
    private TValue? _value;

    [JsonInclude]
    [SerializationTopic(SerializationTopics.Config)]
    public string? Path { get; set; }

    [JsonIgnore]
    public TValue? Value
    {
        get => _value;
        set
        {
            if (value is null)
            {
                if (this is IRequiredConfigRef)
                {
                    throw new InvalidOperationException(
                        $"Required config reference '{typeof(TValue).FullName}' cannot be cleared.");
                }

                _value = null;
                Path = null;
                return;
            }

            _value = value;
            Path = value.Path;
        }
    }

    public Type ValueType => typeof(TValue);

    ConfigEntity? IConfigRef.Value
    {
        get => Value;
        set
        {
            if (value is null)
            {
                Value = null;
                return;
            }

            if (value is TValue val)
            {
                Value = val;
                return;
            }

            throw new ArgumentException(
                $"Expected {typeof(TValue).FullName}, got {value.GetType().FullName}.");
        }
    }

    internal void Resolve(ConfigEntity owner)
    {
        if (Path is null)
        {
            _value = null;
            return;
        }

        string unresolvedPath = Path;
        TValue? val = owner.Root.AllDescendants.OfType<TValue>()
            .FirstOrDefault(c => c.Path == unresolvedPath);

        if (val is null)
        {
            _value = null;
            Path = unresolvedPath;
            return;
        }

        Value = val;
    }

    void IConfigRef.Resolve(ConfigEntity owner) => Resolve(owner);

    internal void SyncPath()
    {
        if (_value is null)
        {
            throw new InvalidOperationException("Cannot synchronize a config reference without a resolved val.");
        }

        Path = _value.Path;
    }

    void IConfigRef.SyncPath() => SyncPath();
}

