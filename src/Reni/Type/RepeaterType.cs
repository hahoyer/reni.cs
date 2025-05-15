using Reni.Context;

namespace Reni.Type;

interface IRepeaterType
{
    Root Root { get; }
    TypeBase ElementType { get; }
    TypeBase IndexType { get; }
    bool IsMutable { get; }
}