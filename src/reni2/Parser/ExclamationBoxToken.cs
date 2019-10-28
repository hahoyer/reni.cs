using hw.DebugFormatter;
using hw.Parser;
using JetBrains.Annotations;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ExclamationBoxToken
        : DumpableObject,
            IParserTokenType<Syntax>,
            ITokenClass,
            IDeclaratorTokenClass
    {
        internal ExclamationBoxToken(Syntax value) => Value = value;

        [CanBeNull]
        Result<Declarator> IDeclaratorTokenClass.Get(Syntax syntax)
        {
            if(!(syntax.Right.TokenClass is IDeclaratorTagProvider provider))
                return new Declarator(null, null, syntax.SourcePart)
                    .Issues(IssueId.UnknownDeclarationTag.Issue(syntax.SourcePart));

            var result = provider.Get(syntax.Right);

            var other = syntax.Left?.Declarator;
            if(other == null)
                return result;

            return result.Target.Combine(other.Target).Issues(result.Issues.plus(other.Issues));
        }

        Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return Syntax.CreateSourceSyntax(left, this, token, Value);
        }

        string IParserTokenType<Syntax>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";
        Syntax Value {get;}
    }
}