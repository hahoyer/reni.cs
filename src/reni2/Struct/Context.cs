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
            var result = Void.CreateResult(category);
            for (var i = 0; i < StatementList.Count; i++)
                result = result.Align(AlignBits).CreateSequence(InternalResult(category, i));
            return result.Align(AlignBits);
        }

        internal Result InternalResult(Category category, int position)
        {
            return CreatePosition(position).Result(category | Category.Type, StatementList[position]).Dereference();
        }

        private Size InternalSize(int position)
        {
            return InternalResult(Category.Size,position).Size;
        }

        internal Result AccessResult(Category category, int position)
        {
            return Type(StatementList[position])
                .Dereference()
                .UnProperty()
                .CreateAssignableRef(RefAlignParam)
                .CreateResult(category, ()=>AccessCode(position));
        }

        private CodeBase AccessCode(int position)
        {
            var offset = Reni.Size.Zero;
            for (var i = position+1; i < StatementList.Count; i++)
                offset += InternalSize(i);
            return new Arg(RefSize).CreateRefPlus(RefAlignParam, offset);
        }

    }
}