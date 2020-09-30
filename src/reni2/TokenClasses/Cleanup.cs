using hw.Parser;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Cleanup : TokenClass, IValueProvider
    {
        public const string TokenId = "~~~";
        public override string Id => TokenId;

        Result<Value> IValueProvider.Get(Syntax syntax)
        {
            var statements = syntax.Left?.ForceStatements;
            if(statements == null)
                statements = new Statement[0];
            var cleanup = syntax.Right?.Value;
            return CompoundSyntax.Create(statements, cleanup, syntax);
        }
    }
}