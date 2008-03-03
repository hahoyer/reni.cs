using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni
{
    internal abstract class StructContainerSearchResult : ReniObject
    {
        internal ContextSearchResult ToContextSearchResult(Struct.Type definingType)
        {
            return new ContextSearchResult(this, definingType);
        }

        internal SearchResult ToSearchResult(Struct.Type definingType)
        {
            return new TypeSearchResult(this, definingType);
        }

        internal abstract Result Visit(Struct.Type definingType, Base callContext,
                                       Category category, Syntax.Base args);
    }

    internal sealed class TypeSearchResult : SearchResult
    {
        [DumpData(true)] private readonly Struct.Type _definingType;
        [DumpData(true)] private readonly StructContainerSearchResult _structContainerSearchResult;

        public TypeSearchResult(StructContainerSearchResult structContainerSearchResult, Struct.Type definingType)
            : base(definingType)
        {
            _structContainerSearchResult = structContainerSearchResult;
            _definingType = definingType;
        }

        /// <summary>
        /// Creates the result for member function searched. Object is provided by use of "Arg" code element
        /// </summary>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        internal protected override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            return _structContainerSearchResult.Visit(_definingType, callContext, category, args);
        }
    }

    internal sealed class ContextSearchResult : ReniObject
    {
        private readonly Struct.Type _definingType;
        [DumpData(true)] private readonly StructContainerSearchResult _structContainerSearchResult;

        public ContextSearchResult(StructContainerSearchResult structContainerSearchResult, Struct.Type definingType)
        {
            _structContainerSearchResult = structContainerSearchResult;
            _definingType = definingType;
        }

        internal Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            return _structContainerSearchResult.Visit(_definingType, callContext, category, args);
        }
    }

    internal sealed class StructAccess : StructContainerSearchResult
    {
        [DumpData(true)] private readonly int _position;

        public StructAccess(int position)
        {
            _position = position;
        }

        internal override Result Visit(Struct.Type definingType, Base callContext,
                                       Category category, Syntax.Base args)
        {
            return definingType.Container.VisitAccessApply(definingType.Context, _position, callContext, category, args);
        }
    }
}