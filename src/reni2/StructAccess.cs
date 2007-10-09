using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni
{
    internal sealed class StructAccess : ReniObject
    {
        [DumpData(true)]
        private readonly int _position;

        public StructAccess(int position)
        {
            _position = position;
        }

        internal StructSearchResult ToContextSearchResult(Struct.Context definingContext)
        {
            return new ContextSearchResult(definingContext,_position);
        }

        internal SearchResult ToSearchResult(Reni.Struct.Type definingType)
        {
            return new TypeSearchResult(definingType, _position);
        }
    }

    internal sealed class ContextSearchResult : StructSearchResult
    {
        [DumpData(true)]
        private readonly int _position;
        [DumpData(true)]
        private readonly Struct.Context _definingContext;

        internal ContextSearchResult(Struct.Context definingContext, int position)
        {
            _position = position;
            _definingContext = definingContext;
        }

        internal override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            return _definingContext.VisitAccessApply(_position, callContext, category, args);
        }
    }

    internal sealed class TypeSearchResult : SearchResult
    {
        [DumpData(true)]
        private readonly int _position;
        [DumpData(true)]
        private readonly Reni.Struct.Type _definingType;

        public TypeSearchResult(Reni.Struct.Type definingType, int position)
            : base(definingType)
        {
            _position = position;
            _definingType = definingType;
        }

        public override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            NotImplementedMethod(callContext, category, args);
            return null;
        }
    }

}