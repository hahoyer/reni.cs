using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;

namespace Reni
{

    internal abstract class StructContainerSearchResult: ReniObject
    {
        [DumpData(true)]
        private readonly DefineableToken _defineableToken;

        public StructContainerSearchResult(DefineableToken defineableToken)
        {
            _defineableToken = defineableToken;
        }

        internal StructSearchResult ToContextSearchResult(Struct.Context definingContext)
        {
            return new ContextSearchResult(this, definingContext);
        }
        internal SearchResult ToSearchResult(Reni.Struct.Type definingType)
        {
            return new TypeSearchResult(this, definingType);
        }

        internal abstract Result Visit(Container definingContainer, Base definingParentContext, Base callContext,
                                       Category category, Syntax.Base args);
    }

    internal sealed class TypeSearchResult : SearchResult
    {
        [DumpData(true)]
        private readonly StructContainerSearchResult _structContainerSearchResult;
        [DumpData(true)]
        private readonly Struct.Type _definingType;

        public TypeSearchResult(StructContainerSearchResult structContainerSearchResult, Struct.Type definingType) : base(definingType)
        {
            _structContainerSearchResult = structContainerSearchResult;
            _definingType = definingType;
        }

        //public override Result VisitApplyFromRef(Base callContext, Category category, Syntax.Base args)
        //{
        //    return _structContainerSearchResult.Visit(_definingType.Container, _definingType.Context, callContext, category, args);
        //}
        /// <summary>
        /// Creates the result for member function searched. Object is provided by use of "Arg" code element
        /// </summary>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            NotImplementedMethod(callContext, category, args);
            return null;
        }
    }

    internal class ContextSearchResult : StructSearchResult
    {
        [DumpData(true)]
        private readonly StructContainerSearchResult _structContainerSearchResult;
        [DumpData(true)]
        private readonly Struct.Context _definingContext;

        public ContextSearchResult(StructContainerSearchResult structContainerSearchResult, Struct.Context definingContext)
        {
            _structContainerSearchResult = structContainerSearchResult;
            _definingContext = definingContext;
        }

        internal override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            return _structContainerSearchResult.Visit(_definingContext.Container, _definingContext.Parent, callContext, category, args);
        }
    }

    internal sealed class StructAccess : StructContainerSearchResult
    {
        [DumpData(true)]
        private readonly int _position;

        public StructAccess(DefineableToken defineableToken, int position) : base(defineableToken)
        {
            _position = position;
        }

        internal override Result Visit(Container definingContainer, Base definingParentContext, Base callContext,
                                       Category category, Syntax.Base args)
        {
            return definingContainer.VisitAccessApply(definingParentContext, _position, callContext, category, args);
        }
    }

    internal sealed class OperationResult : StructContainerSearchResult
    {
        public OperationResult(DefineableToken defineableToken) : base(defineableToken)
        {

        }

        internal override Result Visit(Container definingContainer, Base definingParentContext, Base callContext,
                                       Category category, Syntax.Base args)
        {
            return definingContainer.VisitOperationApply(definingParentContext, callContext, category, args);
        }
    }


}