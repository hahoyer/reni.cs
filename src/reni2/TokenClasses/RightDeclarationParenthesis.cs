using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(DeclarationTokenFactory))]
    sealed class RightDeclarationParenthesis
        : RightParenthesisBase
            , IDeclarerTagProvider
            , IBracketMatch<BinaryTree>,IDeclarerSyntaxFactoryToken

    {
        sealed class Matched
            : DumpableObject
                , IParserTokenType<BinaryTree>
        {
            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
            {
                Tracer.Assert(right == null);
                return left;
            }

            string IParserTokenType<BinaryTree>.PrioTableId => "()";
        }

        public RightDeclarationParenthesis(int level)
            : base(level) { }

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();

        Result<Declarer> IDeclarerTagProvider.Get(BinaryTree binaryTree)
        {
            var bracketKernel = binaryTree.GetBracketKernel(Level);
            var target = bracketKernel.Target;
            if(target != null)
            {
                var items = target
                    .Items
                    .Select(GetDeclarationTag)
                    .ToArray();

                var result = new Declarer
                (
                    items
                        .Select(item => item.Target)
                        .Where(item => item != null)
                        .ToArray(),
                    null,
                    T(binaryTree)
                );

                var issues = items.SelectMany(item => item.Issues).ToArray();

                return result.Issues(issues);
            }
            else
            {
                var issues
                    = T(bracketKernel.Issues, T(IssueId.MissingDeclarationTag.Issue(binaryTree.SourcePart)))
                        .Concat()
                        .ToArray();
                return new Declarer(null, null, T(binaryTree)).Issues(issues);
            }
        }

        static Result<IDeclarationTag> GetDeclarationTag(BinaryTree item)
        {
            if(item.TokenClass is IDeclarationTag result)
                return new Result<IDeclarationTag>(result);

            return new Result<IDeclarationTag>(null, IssueId.InvalidDeclarationTag.Issue(item.SourcePart));
        }

        IDeclarerSyntaxFactory IDeclarerSyntaxFactoryToken.Provider => SyntaxFactory.ComplexDeclarer;
    }
}