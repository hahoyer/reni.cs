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
        internal override bool IsRefOperation { get { return true; } }

        internal override Result VisitRefOperationApply(Context.Base context, Category category, Syntax.Base args, Ref definingType)
        {
            if(category.HasCode || category.HasRefs)
                return definingType.AssignmentOperator(args.Visit(context,category|Category.Type));
            return Type.Base.CreateVoid.CreateResult(category);

        }
    }
}
