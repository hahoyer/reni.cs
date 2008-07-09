using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    sealed internal class ColonEqual : Defineable, IConverter<IFeature, AssignableRef>
    {
        internal override string Name { get { return ":="; } }

        internal override SearchResult<IConverter<IFeature, AssignableRef>> SearchFromAssignableRef()
        {
            return SearchResult<IConverter<IFeature, AssignableRef>>.Success(this, this);
        }

        public IFeature Convert(AssignableRef type)
        {
            return type.AssignmentFeature;
        }
    }
}
