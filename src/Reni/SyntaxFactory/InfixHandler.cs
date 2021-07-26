using hw.DebugFormatter;
using hw.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxFactory
{
    class InfixHandler : DumpableObject, IValueProvider
    {
        readonly bool? HasLeft;
        readonly bool? HasRight;

        public InfixHandler(bool? hasLeft = null, bool? hasRight = null)
        {
            HasLeft = hasLeft;
            HasRight = hasRight;
        }

        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, Anchor anchor)
        {
            var left = factory.GetValueSyntax(target.Left);
            var right = factory.GetValueSyntax(target.Right);

            switch(HasLeft)
            {
                case true:
                    left ??= ErrorToken.CreateSyntax(IssueId.MissingLeftOperand, target.Token.SourcePart().Start);
                    break;
                case false:
                    if(left != null)
                        left = ErrorToken.CreateSyntax(IssueId.InvalidLeftOperand, left);
                    break;
            }

            switch(HasRight)
            {
                case true:
                    right ??= ErrorToken.CreateSyntax(IssueId.MissingRightOperand, target.Token.SourcePart().End);
                    break;
                case false:
                    if(right != null)
                        right= ErrorToken.CreateSyntax(IssueId.InvalidRightOperand, right);
                    break;
            }

            return left
                .GetInfixSyntax(target, right, Anchor.Create(target).Combine(anchor));
        }
    }
}