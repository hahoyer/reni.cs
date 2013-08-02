using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class OperationPrefixFeature 
        : ReniObject
    {
        [EnableDump]
        private readonly BitType.IPrefix _definable;

        private readonly BitType _bitType;

        public OperationPrefixFeature(BitType bitType, BitType.IPrefix definable)
        {
            _bitType = bitType;
            _definable = definable;
        }
    }
}