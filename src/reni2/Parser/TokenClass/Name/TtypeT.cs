using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    sealed class TtypeT: Defineable
    {
        /// <summary>
        /// Obtain result
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <param name="definingType">Type of the defining.</param>
        /// <returns></returns>
        internal override Result VisitDefaultOperationApply(Context.Base context, Category category, Syntax.Base args, Type.Base definingType)
        {
            if (args == null)
                return definingType.TypeOperator(category);
            Result argResult = args.Visit(context, category | Category.Type);
            return definingType.ApplyTypeOperator(argResult);
        }
        /// <summary>
        /// Gets the type operation.
        /// </summary>
        /// <value>The type operation.</value>
        /// created 07.01.2007 16:24
        internal override bool IsDefaultOperation { get { return true; } }

    }
}
