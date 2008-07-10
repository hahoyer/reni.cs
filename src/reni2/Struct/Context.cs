using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Context : Reni.Context.Child
    {
        private readonly DictionaryEx<int, ContextAtPosition> _contextAtPositionCache = new DictionaryEx<int, ContextAtPosition>();
        private readonly SimpleCache<Type> _typeCache = new SimpleCache<Type>();
        internal readonly Container Container;

        internal Context(ContextBase contextBase, Container container) : base(contextBase)
        {
            Container = container;
        }

        [DumpData(false)]
        internal List<ICompileSyntax> StatementList { get { return Container.List; } }
        [DumpData(false)]
        internal int IndexSize { get { return Container.IndexSize; } }

        internal ContextAtPosition CreatePosition(int position)
        {
            return _contextAtPositionCache.Find(position, () => new ContextAtPosition(this, position));
        }

        internal Type CreateType()
        {
            return _typeCache.Find(() => new Type(this));
        }

        internal Result InternalResult(Category category)
        {
            return InternalResult(category, 0, StatementList.Count);
        }

        internal Result InternalResult(Category category, int fromPosition, int fromNotPosition)
        {
            var result = Void.CreateResult(category);
            for (var i = fromPosition; i < fromNotPosition; i++)
                result = result.CreateSequence(InternalResult(category, i));
            return result;
        }

        private Result InternalResult(Category category, int position)
        {
            return CreatePosition(position)
                .Result(category | Category.Type, StatementList[position])
                .AutomaticDereference()
                .Align(AlignBits);
        }

        private Size InternalSize(int position)
        {
            return InternalResult(Category.Size,position).Size;
        }

        internal Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Type(StatementList[position])
                .AutomaticDereference()
                .UnProperty()
                .CreateAssignableRef(refAlignParam)
                .CreateResult(category, ()=>AccessCode(position,refAlignParam));
        }

        private CodeBase AccessCode(int position, RefAlignParam refAlignParam)
        {
            var offset = Reni.Size.Zero;
            for (var i = position + 1; i < StatementList.Count; i++)
                offset += InternalSize(i);
            return new Arg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, offset);
        }

        internal override string DumpShort()
        {
            return "context."+ObjectId + "("+ Container.DumpShort()+")";
        }

        internal override Result CreateThisRefResult(Category category)
        {
            return CreateType()
                .CreateRef(RefAlignParam)
                .CreateResult(category,() => CodeBase.CreateTopRef(RefAlignParam));
        }

    }
}