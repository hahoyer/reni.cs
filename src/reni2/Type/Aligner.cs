using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    internal sealed class Aligner : Child
    {
        private readonly int _alignBits;

        public Aligner(TypeBase target, int alignBits)
            : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(-130);
        }

        [DisableDump]
        protected override bool IsInheritor { get { return true; } }
        [DisableDump]
        internal int AlignBits { get { return _alignBits; } }
        [DisableDump]
        internal override Size UnrefSize { get { return Parent.UnrefSize; } }
        [DisableDump]
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }

        internal override int SequenceCount(TypeBase elementType) { return Parent.SequenceCount(elementType); }
        protected override Size GetSize() { return Parent.Size.Align(AlignBits); }
        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        internal override TypeBase TypeForTypeOperator() { return Parent.TypeForTypeOperator(); }
        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }
        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return Parent.IsConvertableTo(dest, conversionParameter); }
        internal override bool HasConverterTo(TypeBase dest) { return Parent.HasConverterTo(dest); }
        internal override string DumpShort() { return base.DumpShort() + "(" + Parent.DumpShort() + ")"; }

        internal override AutomaticReferenceType SpawnReference(RefAlignParam refAlignParam)
        {
            if (_alignBits == refAlignParam.AlignBits)
                return Parent.SpawnReference(refAlignParam);
            return base.SpawnReference(refAlignParam);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            return Parent
                .ConvertTo(category, dest)
                .ReplaceArg(CreateUnalignedArgResult(category));
        }

        private Result CreateUnalignedArgResult(Category category)
        {
            return Parent.Result
                (
                    category,
                    () => ArgCode().BitCast(Parent.Size)
                );
        }

    }
}