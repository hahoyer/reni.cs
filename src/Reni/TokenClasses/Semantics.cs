using hw.DebugFormatter;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.TokenClasses;

sealed class Semantics : DumpableObject
{
    internal readonly Dictionary<ContextBase, IImplementation> Declaration = new ();
    public TypeBase Result;
}