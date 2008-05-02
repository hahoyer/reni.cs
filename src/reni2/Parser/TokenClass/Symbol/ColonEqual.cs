using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    sealed internal class ColonEqual : Defineable, IRefFeature
    {
        internal override string Name { get { return ":="; } }

        internal override SearchResult<IRefFeature> SearchFromRef()
        {
            return SearchResult < IRefFeature >.Success(this,this);
        }

        public IFeature Convert(Ref @ref)
        {
            return @ref.AssignmentOperatorFeatureObject();
        }
    }
}
