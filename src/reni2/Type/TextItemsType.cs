using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Sequence;

namespace Reni.Type
{
    internal sealed class TextItemsType : TagChild<Array>
    {
        [DisableDump]
        public readonly ISuffixFeature ToNumberOfBaseFeature;

        public TextItemsType(Array parent)
            : base(parent)
        {
            ToNumberOfBaseFeature = new ToNumberOfBaseFeature(this);
        }

        [DisableDump]
        protected override string TagTitle { get { return "text_items"; } }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => Parent);
            if (!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal override int? SmartArrayLength(TypeBase elementType) { return Parent.SmartArrayLength(elementType); }

        internal Result ConcatArrays(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, objectReference, argsType);
            try
            {
                var result = Parent.InternalConcatArrays(category.Typed, objectReference, argsType);
                Dump("result", result);
                BreakExecution();

                var type = (Array) result.Type;
                return ReturnMethodDump(type.UniqueTextItemsType.Result(category, result));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result DumpPrintTextResult(Category category)
        {
            return Void.Result
                (category
                 , DumpPrintCode
                 , CodeArgs.Arg
                );
        }

        private CodeBase DumpPrintCode()
        {
            return UniquePointer
                .ArgCode
                .Dereference(Size)
                .DumpPrintText(Parent.Element.Size);
        }
    }
}