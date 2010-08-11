using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal abstract class FeatureBase : ReniObject, ISearchPath<IFeature, Type.Sequence>
    {
        [DumpData(true)]
        protected internal readonly ISequenceOfBitBinaryOperation Definable;

        protected FeatureBase(ISequenceOfBitBinaryOperation definable)
        {
            Definable = definable;
        }

        IFeature ISearchPath<IFeature, Type.Sequence>.Convert(Type.Sequence type) { return type.Feature(this); }

        internal abstract TypeBase ResultType(int objSize, int argsSize);
    }
}