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

        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
        {
            if(HasLeft != null)
                (target.Left == null != HasLeft).Assert();
            if(HasRight != null)
                (target.Right == null != HasRight).Assert();

            return factory.GetInfixSyntax(target, frameItems);
        }
    }
}