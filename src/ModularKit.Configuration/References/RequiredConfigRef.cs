using ModularKit.Configuration;
using ModularKit.Configuration.Serialization;

namespace ModularKit.Configuration.References;

[SerializationTopic(SerializationTopics.Config)]
public class RequiredConfigRef<TValue> : ConfigRefBase<TValue>, IRequiredConfigRef
    where TValue : ConfigEntity
{
}

