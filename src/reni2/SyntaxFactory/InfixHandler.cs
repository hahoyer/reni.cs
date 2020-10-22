using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

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

        Result<ValueSyntax> IValueProvider.Get
            (BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory)
        {
            if(HasLeft != null)
                (target.Left == null != HasLeft).Assert();
            if(HasRight != null)
                (target.Right == null != HasRight).Assert();

            return factory.GetInfixSyntax(target);
        }

        UsageTree IValueProvider.GetUsage
            (BinaryTree leftAnchor, BinaryTree target, Factory factory)
            => new UsageTree {};
    }
}