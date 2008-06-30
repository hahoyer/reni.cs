using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal class AtFeature : IContextFeature, IFeature
    {
        private readonly Container _container;
        private readonly ContextBase _parentContext;

        public AtFeature(Container container, ContextBase parentContext)
        {
            _parentContext = parentContext;
            _container = container;
        }

        public Result VisitApply(ContextBase callContext, Category category, ICompileSyntax args)
        {
            return _container.VisitOperationApply(_parentContext, callContext, category, args);
        }

        public Result Result(ContextBase callContext, Category category, ICompileSyntax args, Ref callObject)
        {
            return _container.VisitOperationApply(_parentContext, callContext, category, args);
        }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            return _container.ApplyAtFeature(_parentContext, callContext, category, @object, args);
        }
    }
}