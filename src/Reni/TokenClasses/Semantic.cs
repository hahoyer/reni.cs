using hw.DebugFormatter;
using Reni.Context;
using Reni.Feature;

namespace Reni.TokenClasses;

sealed class Semantic : DumpableObject
{
    internal readonly Dictionary<ContextBase, IImplementation> Declaration = new ();
}