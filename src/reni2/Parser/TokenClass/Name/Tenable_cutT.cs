using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class Tenable_cutT: Defineable
    {
        internal override bool IsDefaultOperation { get { return true; } }

        internal override Result VisitDefaultOperationApply(Context.Base callContext, Category category, Syntax.Base args, Type.Base definingType)
        {
            if (args == null)
                return definingType.CreateEnableCut().CreateArgResult(category);
            return base.VisitDefaultOperationApply(callContext, category, args, definingType);
        }
    }
}