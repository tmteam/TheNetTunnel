using System.IO;

namespace TNT.Presentation.Serializers;

public abstract class SerializerBase<T> : ISerializer<T>
{
    public abstract void SerializeT(T obj, MemoryStream stream);

    public virtual void Serialize(object obj, MemoryStream stream)
    {
        SerializeT((T) obj, stream);
    }

    public int? Size { get; protected set; }
}