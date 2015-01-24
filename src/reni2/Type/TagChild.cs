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
            : base(parent) {}

        [DisableDump]
        protected abstract string TagTitle { get; }
        [DisableDump]
        internal override string DumpPrintText => "(" + Parent.DumpPrintText + ")" + TagTitle;
        [DisableDump]
        internal override sealed bool Hllw => Parent.Hllw;
        [DisableDump]
        internal override sealed TypeBase CoreType => Parent.CoreType;
        protected override sealed Size GetSize() => Parent.Size;
        protected override string GetNodeDump() => Parent.NodeDump + "[" + TagTitle + "]";
        internal override sealed Result Destructor(Category category) => Parent.Destructor(category);
        internal override sealed Result ArrayDestructor(Category category, int count) => Parent.ArrayDestructor(category, count);
        internal override sealed Result Copier(Category category) => Parent.Copier(category);
        internal override sealed Result ArrayCopier(Category category, int count) => Parent.ArrayCopier(category, count);

        protected override Result ParentConversionResult(Category category) => Parent.Result(category, ArgResult);

        [DisableDump]
        internal override IEnumerable<ISimpleFeature> StripConversions
        {
            get { yield return Extension.SimpleFeature(ParentConversionResult); }
        }
    }
}