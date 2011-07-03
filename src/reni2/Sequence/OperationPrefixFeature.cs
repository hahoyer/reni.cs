using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class OperationPrefixFeature : ReniObject, ISearchPath<IPrefixFeature, SequenceType>
    {
        [EnableDump]
        private readonly ISequenceOfBitPrefixOperation _definable;

        private readonly Bit _bit;

        public OperationPrefixFeature(Bit bit, ISequenceOfBitPrefixOperation definable)
        {
            _bit = bit;
            _definable = definable;
        }

        IPrefixFeature ISearchPath<IPrefixFeature, SequenceType>.Convert(SequenceType type) { return type.PrefixFeature(_definable); }
    }
}