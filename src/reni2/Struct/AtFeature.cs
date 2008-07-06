using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

namespace Reni.Struct
{
    internal class AtFeature 
    {
        private readonly Container _container;
        private readonly ContextBase _parentContext;

        public AtFeature(Container container, ContextBase parentContext)
        {
            _parentContext = parentContext;
            _container = container;
        }
    }
}