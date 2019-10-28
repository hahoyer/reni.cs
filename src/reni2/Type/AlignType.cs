using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
            StopByObjectIds(-9);
            Tracer.Assert(Parent.IsAligningPossible, Parent.Dump);
        }

        [DisableDump]
        internal override string DumpPrintText
            => "(" + Parent.DumpPrintText + ")" + AlignToken.TokenId + _alignBits;

        [DisableDump]
        internal override bool Hllw => Parent.Hllw;

        protected override IReference GetForcedReferenceForCache() => Parent.ForcedReference;

        protected override Result DeAlign(Category category) => Mutation(Parent) & category;
        protected override PointerType GetPointerForCache() => Parent.ForcedPointer;

        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        IEnumerable<string> InternalDeclarationOptions => Parent.DeclarationOptions;

        [DisableDump]
        protected override IEnumerable<IConversion> RawSymmetricConversions
            =>
                base.RawSymmetricConversions.Concat
                    (new IConversion[] {Feature.Extension.Conversion(UnalignedResult)});

        [DisableDump]
        internal override bool IsAligningPossible => false;
        [DisableDump]
        internal override bool IsPointerPossible => false;

        protected override Size GetSize() => Parent.Size.Align(_alignBits);

        internal override int? SmartArrayLength(TypeBase elementType)
            => Parent.SmartArrayLength(elementType);

        internal override Result Copier(Category category) => Parent.Copier(category);

        internal override Result ApplyTypeOperator(Result argResult)
            => Parent.ApplyTypeOperator(argResult);

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + Parent.NodeDump + ")";

        protected override Result ParentConversionResult(Category category)
            => UnalignedResult(category);

        public Result UnalignedResult(Category category)
            => Parent.Result(category, () => ArgCode.BitCast(Parent.Size));
    }
}