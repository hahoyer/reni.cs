using System.Collections.Generic;
using JetBrains.Annotations;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class Extension
    {
        public static IEnumerable<ISourcePartEdit> GetWhiteSpaceEdits
            (this BinaryTree target, Configuration configuration, int lineBreakCount, Syntax.Anchor kind)
        {
            if(target != null)
                yield return new WhiteSpaces
                (
                    target.WhiteSpaces
                    , configuration
                    , target.SeparatorRequests
                    , kind.TargetPosition
                    , lineBreakCount
                );
        }

        [UsedImplicitly]
        static TValue[] T<TValue>(params TValue[] value) => value;
    }
}