using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(DeclarationTokenFactory))]
    sealed class RightDeclarationParenthesis
        : RightParenthesisBase,
            IDeclarerTagProvider,
            IBracketMatch<BinaryTree>

    {
        sealed class Matched : DumpableObject, IParserTokenType<BinaryTree>
        {
            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
            {
                Tracer.Assert(right == null);
                return left;
            }

            string IParserTokenType<BinaryTree>.PrioTableId => "()";
        }

        public RightDeclarationParenthesis(int level)
            : base(level) {}

        [Obsolete("",true)]
        Result<Declarer> IDeclarerTagProvider.Get(BinaryTree binaryTree)
        {
            var bracketKernel = binaryTree.Left.GetBracketKernel(Level, binaryTree);
            var target = bracketKernel.Target;
            if(target != null)
            {
                var items = target
                    .Option
                    .Items
                    .Select(GetDeclarationTag)
                    .ToArray();

                var result = new Declarer
                (
                    items.Select(item => item.Target).Where(item => item != null).ToArray(),
                    null,
                    binaryTree.SourcePart
                );
                var issues = items.SelectMany(item => item.Issues).ToArray();
                return result.Issues(issues);
            }
            else
            {
                var issues = bracketKernel.Issues.plus(IssueId.MissingDeclarationTag.Issue(binaryTree.SourcePart));
                return new Declarer(null, null, binaryTree.SourcePart).Issues(issues);
            }
        }

        [Obsolete("",true)]
        static Result<IDeclarationTag> GetDeclarationTag(BinaryTree item)
        {
            var result = item.Option.DeclarationTag;
            if(result != null)
                return new Result<IDeclarationTag>(result);

            return new Result<IDeclarationTag>(null, IssueId.InvalidDeclarationTag.Issue(item.SourcePart));
        }

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value {get;} = new Matched();
    }
}