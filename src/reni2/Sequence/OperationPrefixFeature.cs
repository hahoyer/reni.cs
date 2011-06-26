using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class OperationPrefixFeature : ReniObject, ISearchPath<IPrefixFeature, BaseType>
    {
        [EnableDump]
        private readonly ISequenceOfBitPrefixOperation _definable;

        private readonly Bit _bit;

        public OperationPrefixFeature(Bit bit, ISequenceOfBitPrefixOperation definable)
        {
            _bit = bit;
            _definable = definable;
        }

        IPrefixFeature ISearchPath<IPrefixFeature, BaseType>.Convert(BaseType type) { return type.PrefixFeature(_definable); }
    }
}