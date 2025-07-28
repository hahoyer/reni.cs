using System.Diagnostics;
using Reni.Feature;

namespace Reni.Type;

abstract partial class TypeBase
{
    internal sealed class ConversionSetup(TypeBase parent)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly TypeBase Parent = parent;

        [DisableDump]
        public IEnumerable<IConversion> Symmetric 
            => Parent.Cache.SymmetricConversions.Value;

        [DisableDump]
        internal IConversion[] NextStep
            => SymmetricClosure.Union(Strip).ToArray();

        [DisableDump]
        internal IConversion[] SymmetricClosure
            => new SymmetricClosureService(Parent).Results.ToArray();

        internal IConversion[] RawSymmetric 
            => Parent.GetSymmetricConversions().ToArray();

        internal IConversion[] Strip => Parent.GetStripConversions().ToArray();
        internal IConversion[] StripFromPointer => Parent.GetStripConversionsFromPointer().ToArray();

    }
}
