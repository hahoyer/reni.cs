using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.Numeric;

namespace Reni.Type
{
    sealed class AlignType
        : Child<TypeBase>
    {
        [DisableDump]
        readonly int _alignBits;

        public AlignType(TypeBase target, int alignBits)
            : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(-9);
            Tracer.Assert(Parent.IsAligningPossible, Parent.Dump);
        }

        [DisableDump]
        internal override string DumpPrintText => "(" + Parent.DumpPrintText + ")" + AlignToken.TokenId + _alignBits;

        [DisableDump]
        internal override IReference CheckedReference => Parent.CheckedReference;

        [DisableDump]
        internal override bool Hllw => Parent.Hllw;

        [DisableDump]
        internal override IReference ForcedReference => Parent.ForcedReference;

        protected override Result DeAlign(Category category) => Parent.Result(category, ArgResult);
        internal override PointerType ForcedPointerForCache() => Parent.ForcedPointer;

        protected override IEnumerable<ISimpleFeature> RawSymmetricConversions
            => base.RawSymmetricConversions.Concat(new ISimpleFeature[] {Feature.Extension.SimpleFeature(UnalignedResult)});

        [DisableDump]
        internal override bool IsAligningPossible => false;
        [DisableDump]
        internal override bool IsPointerPossible => false;

        protected override Size GetSize() => Parent.Size.Align(_alignBits);
        internal override int? SmartArrayLength(TypeBase elementType) => Parent.SmartArrayLength(elementType);
        internal override Result Destructor(Category category) => Parent.Destructor(category);
        internal override Result Copier(Category category) => Parent.Copier(category);
        internal override Result ApplyTypeOperator(Result argResult) => Parent.ApplyTypeOperator(argResult);
        protected override string GetNodeDump() => base.GetNodeDump() + "(" + Parent.NodeDump + ")";

        protected override Result ParentConversionResult(Category category) => UnalignedResult(category);

        public Result UnalignedResult(Category category) { return Parent.Result(category, () => ArgCode.BitCast(Parent.Size)); }
    }
}