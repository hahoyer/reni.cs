using hw.DebugFormatter;

namespace Reni;

abstract class Singleton<T> : DumpableObject
    where T : DumpableObject, new()
{
    internal static readonly T Instance = new();

    protected Singleton(int? objectId = null)
        : base(objectId) { }
}