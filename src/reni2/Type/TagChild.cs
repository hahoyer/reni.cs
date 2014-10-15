using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    abstract class TagChild<TParent> : Child<TParent>
        where TParent : TypeBase
    {
        protected TagChild(TParent parent)
            : base(parent)
        {}

        [DisableDump]
        protected abstract string TagTitle { get; }
        [DisableDump]
        internal override string DumpPrintText { get { return Parent.DumpPrintText + " #(# " + TagTitle + " #)#"; } }
        [DisableDump]
        internal override sealed bool Hllw { get { return Parent.Hllw; } }
        [DisableDump]
        internal override sealed TypeBase CoreType { get { return Parent.CoreType; } }
        protected override sealed Size GetSize() { return Parent.Size; }
        protected override string GetNodeDump() { return Parent.NodeDump + "[" + TagTitle + "]"; }
        internal override sealed Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override sealed Result ArrayDestructor(Category category, int count)
        {
            return Parent.ArrayDestructor(category, count);
        }
        internal override sealed Result Copier(Category category) { return Parent.Copier(category); }
        internal override sealed Result ArrayCopier(Category category, int count) { return Parent.ArrayCopier(category, count); }
        protected override Result ParentConversionResult(Category category)
        {
            return Parent.Result(category, ArgResult);
        }
        internal Result PointerConversionResult(Category category) { return PointerConversionResult(category, Parent); }

        [DisableDump]
        protected override IEnumerable<ISimpleFeature> Conversions
        {
            get { return base.Conversions.Concat(new ISimpleFeature[] { Extension.SimpleFeature(ParentConversionResult) }); }
        }
    }
}