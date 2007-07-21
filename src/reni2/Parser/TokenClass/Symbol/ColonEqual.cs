using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    sealed internal class ColonEqual: Defineable
    {
        /// <summary>
        /// Gets the ref operation.
        /// </summary>
        /// <value>The ref operation.</value>
        /// created 14.02.2007 02:17
        public override SearchResult RefOperation(Ref obj)
        {
            return new FoundAssignementResult(obj);
        }
    }

    internal class FoundAssignementResult : SearchResult
    {
        [DumpData(true)]
        private readonly Ref _obj;

        public FoundAssignementResult(Ref obj)
            : base(obj)
        {
            _obj = obj;
        }

        /// <summary>
        /// Obtain result
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public override Result VisitApply(Context.Base context, Category category, Syntax.Base args)
        {
            if(category.HasCode || category.HasRefs)
                return _obj.AssignementOperator(args.Visit(context,category|Category.Type));
            return Type.Base.CreateVoid.CreateResult(category);

        }
    }
}
