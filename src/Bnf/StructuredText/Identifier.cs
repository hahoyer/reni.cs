using hw.Parser;

namespace Bnf.StructuredText
{
    [BelongsTo(typeof(ScannerTokenFactory))]
    sealed class Identifier : ScannerTokenType
    {
        protected override string Id => "identifier";

        protected override IParserTokenType<TSourcePart> GetParserTokenType<TSourcePart>(string id)
        {
            NotImplementedMethod(id);
            return null;
        }
    }
}