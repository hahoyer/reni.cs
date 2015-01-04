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
        , IFeatureInheritor
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
        internal override string DumpPrintText { get { return "("+Parent.DumpPrintText + ")"+AlignToken.Id + _alignBits; } }

        [DisableDump]
        internal override IReferenceType ReferenceType { get { return Parent.ReferenceType; } }

        [DisableDump]
        internal override bool Hllw { get { return Parent.Hllw; } }

        [DisableDump]
        internal override IReferenceType UniquePointerType { get { return Parent.UniquePointerType; } }

        protected override Result DeAlign(Category category) { return Parent.Result(category, ArgResult); }

        protected override IEnumerable<ISimpleFeature> ObtainRawReflexiveConversions()
        {
            return base.ObtainRawReflexiveConversions().Concat(new ISimpleFeature[] {Extension.SimpleFeature(UnalignedResult)});
        }

        [DisableDump]
        internal override bool IsAligningPossible { get { return false; } }

        Result IFeatureInheritor.Source(Category category) { return UnalignedResult(category); }

        protected override Size GetSize() { return Parent.Size.Align(_alignBits); }
        internal override int? SmartSequenceLength(TypeBase elementType) { return Parent.SmartSequenceLength(elementType); }
        internal override int? SmartArrayLength(TypeBase elementType) { return Parent.SmartArrayLength(elementType); }
        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }
        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + Parent.NodeDump + ")"; }

        protected override Result ParentConversionResult(Category category) { return UnalignedResult(category); }

        public Result UnalignedResult(Category category) { return Parent.Result(category, () => ArgCode.BitCast(Parent.Size)); }
    }
}