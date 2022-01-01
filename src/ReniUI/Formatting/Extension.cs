using System.Collections.Generic;
using JetBrains.Annotations;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class Extension
    {
        public static IEnumerable<ISourcePartEdit> GetWhiteSpaceEdits
            (this BinaryTree target, Configuration configuration, int lineBreakCount, string targetPositionForDebug)
        {
            if(target != null)
                yield return new WhiteSpaces
                (
                    target.WhiteSpaces
                    , configuration
                    , target.SeparatorRequests
                    , targetPositionForDebug
                    , lineBreakCount
                );
        }

        [UsedImplicitly]
        static TValue[] T<TValue>(params TValue[] value) => value;
    }
}