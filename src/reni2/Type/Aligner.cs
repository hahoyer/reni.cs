using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni.Type
{
    /// <summary>
    ///     Performs alignement by extending the number of bytes a type uses.
    /// </summary>
    [Serializable]
    internal class Aligner : Child
    {
        private readonly int _alignBits;

        public Aligner(TypeBase target, int alignBits)
            : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(-130);
        }

        internal int AlignBits { get { return _alignBits; } }
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }

        internal override int SequenceCount(TypeBase elementType) { return Parent.SequenceCount(elementType); }

        protected override Size GetSize() { return Parent.Size.Align(AlignBits); }

        internal override Size UnrefSize { get { return Parent.UnrefSize; } }

        internal override Result Destructor(Category category) { return Parent.Destructor(category); }

        internal override Result Copier(Category category) { return Parent.Copier(category); }

        internal override TypeBase TypeForTypeOperator() { return Parent.TypeForTypeOperator(); }

        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return Parent.IsConvertableTo(dest, conversionParameter); }

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
                    () => CodeBase.Arg(Size).CreateBitCast(Parent.Size)
                );
        }

        internal override bool HasConverterTo(TypeBase dest) { return Parent.HasConverterTo(dest); }

        internal override string DumpShort() { return "aligner(" + Parent.DumpShort() + ")"; }

        protected override bool IsInheritor { get { return true; } }
    }
}