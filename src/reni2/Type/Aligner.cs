using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    /// <summary>
    /// Performs alignement by extending the number of bytes a type uses.
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

        internal override int GetSequenceCount(TypeBase elementType) { return Parent.GetSequenceCount(elementType); }

        protected override Size GetSize()
        {
            return Parent.Size.Align(AlignBits);
        }

        internal override Size UnrefSize { get { return Parent.UnrefSize; } }

        internal override Result Destructor(Category category)
        {
            return Parent.Destructor(category);
        }

        internal override Result Copier(Category category)
        {
            return Parent.Copier(category);
        }

        internal override TypeBase GetEffectiveType() { return Parent.GetEffectiveType(); }

        internal override Result ApplyTypeOperator(Result argResult)
        {
            return Parent.ApplyTypeOperator(argResult);
        }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            return Parent.IsConvertableTo(dest, conversionFeature);
        }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            return Parent
                .ConvertTo(category, dest)
                .ReplaceArg(CreateUnalignedArgResult(category));
        }

        private Result CreateUnalignedArgResult(Category category)
        {
            return Parent.CreateResult
                (
                category,
                () => CodeBase.CreateArg(Size).CreateBitCast(Parent.Size)
                );
        }

        internal override bool HasConverterTo(TypeBase dest)
        {
            return Parent.HasConverterTo(dest);
        }

        internal override string DumpShort()
        {
            return "aligner(" + Parent.DumpShort() + ")";
        }

        protected override bool IsInheritor { get { return true; } }
    }
}