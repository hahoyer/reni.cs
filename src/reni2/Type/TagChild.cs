using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;

namespace Reni.Type
{
    [Serializable]
    internal abstract class TagChild : Child
    {
        protected TagChild(TypeBase parent)
            : base(parent) { }

        protected TagChild(int objectId, TypeBase parent)
            : base(objectId, parent) { }

        protected abstract string TagTitle { get; }

        protected override Size GetSize() { return Parent.Size; }

        internal override string DumpShort() { return Parent.DumpShort() + "[" + TagTitle + "]"; }

        internal override string DumpPrintText { get { return Parent.DumpPrintText + " #(# " + TagTitle + " #)#"; } }

        internal override Result Destructor(Category category) { return Parent.Destructor(category); }

        internal override Result ArrayDestructor(Category category, int count) { return Parent.ArrayDestructor(category, count); }

        internal override Result Copier(Category category) { return Parent.Copier(category); }

        internal override Result ArrayCopier(Category category, int count) { return Parent.ArrayCopier(category, count); }

        protected override Result ConvertToImplementation(Category category, TypeBase dest) { return Parent.ConvertTo(category, dest); }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return Parent.IsConvertableTo(dest, conversionParameter); }
        protected override bool IsInheritor { get { return true; } }
    }
}