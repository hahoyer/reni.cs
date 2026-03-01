using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type;

static class ReferenceExtension
{
    extension(IReference target)
    {
        internal TypeBase Type => (TypeBase)target;
    }

    extension(IContextReference target)
    {
        internal Size Size => (target as TypeBase)?.OverView.Size
            ?? Root.DefaultRefAlignParam.RefSize;
    }
}
