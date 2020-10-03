using hw.DebugFormatter;
using hw.Parser;
using JetBrains.Annotations;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ExclamationBoxToken
        : DumpableObject,
            IParserTokenType<BinaryTree>,
            ITokenClass,
            IDeclarerTokenClass
    {
        internal ExclamationBoxToken(BinaryTree value) => Value = value;

        [CanBeNull]
        Result<Declarer> IDeclarerTokenClass.Get(BinaryTree binaryTree)
        {
            if(!(binaryTree.Right.TokenClass is IDeclarerTagProvider provider))
                return new Declarer(null, null, T(binaryTree))
                    .Issues(IssueId.UnknownDeclarationTag.Issue(binaryTree.SourcePart));

            var result = provider.Get(binaryTree.Right);

            var other = binaryTree.Left?.Declarer;
            if(other == null)
                return result;

            return result.Target.Combine(other.Target).Issues(result.Issues.plus(other.Issues));
        }

        BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
        {
            Tracer.Assert(right == null);
            return BinaryTree.Create(left, this, token, Value);
        }

        string IParserTokenType<BinaryTree>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";
        BinaryTree Value {get;}
    }
}