using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    sealed class StableReferenceType
        : TagChild<PointerType>
    {
        internal StableReferenceType(PointerType parent)
            : base(parent) { }

        [DisableDump]
        internal override bool IsAligningPossible => false;
        [DisableDump]
        internal override bool IsPointerPossible => false;
        [DisableDump]
        protected override IEnumerable<IConversion> StripConversions
            => base.StripConversions
                .Concat(new[] {Feature.Extension.Conversion(ConvertToPointer)});
        [DisableDump]
        internal override TypeBase Weaken => Parent;

        Result ConvertToPointer(Category category) => Mutation(Parent) & category;

        protected override string TagTitle => "stable";
    }
}