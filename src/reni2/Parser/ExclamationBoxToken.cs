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
            IDeclarerTokenClass
    {
        internal ExclamationBoxToken(Syntax value) => Value = value;

        [CanBeNull]
        Result<Declarer> IDeclarerTokenClass.Get(Syntax syntax)
        {
            if(!(syntax.Right.TokenClass is IDeclarerTagProvider provider))
                return new Declarer(null, null, syntax.SourcePart)
                    .Issues(IssueId.UnknownDeclarationTag.Issue(syntax.SourcePart));

            var result = provider.Get(syntax.Right);

            var other = syntax.Left?.Declarer;
            if(other == null)
                return result;

            return result.Target.Combine(other.Target).Issues(result.Issues.plus(other.Issues));
        }

        Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return Syntax.Create(left, this, token, Value);
        }

        string IParserTokenType<Syntax>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";
        Syntax Value {get;}
    }
}