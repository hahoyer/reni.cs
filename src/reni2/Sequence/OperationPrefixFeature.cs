using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class OperationPrefixFeature 
        : ReniObject
        , ISearchPath<IPrefixFeature, SequenceType>
        , ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType>
    {
        [EnableDump]
        private readonly BitType.IPrefix _definable;

        private readonly BitType _bitType;

        public OperationPrefixFeature(BitType bitType, BitType.IPrefix definable)
        {
            _bitType = bitType;
            _definable = definable;
        }

        IPrefixFeature ISearchPath<IPrefixFeature, SequenceType>.Convert(SequenceType type) { return type.PrefixFeature(_definable); }
        ISearchPath<IPrefixFeature, SequenceType> ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType>.Convert(ArrayType type) { return this; }
    }
}