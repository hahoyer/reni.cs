using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class T_A_T_T : Defineable, IStructFeature
    {
        internal override string Name { get { return "_A_T_"; } }

        internal override SearchResult<IStructFeature> SearchFromStruct()
        {
            return SearchResult<IStructFeature>.Success(this, this);
        }

        public IContextFeature Convert(ContextAtPosition contextAtPosition)
        {
            return contextAtPosition.AtFeatureObject();
        }

        public IFeature Convert(Struct.Type type)
        {
            return type.AtFeatureObject();
        }
    }
}