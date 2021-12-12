using System.Collections.Generic;
using hw.Parser;
using JetBrains.Annotations;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class Extension
    {
        public static IEnumerable<ISourcePartEdit> GetWhiteSpaceEdits
            (this BinaryTree target, Configuration configuration, int lineBreakCount)
        {
            if(target == null)
                yield break;
            var isSeparatorRequired = target.IsSeparatorRequired;
            var token = target.WhiteSpaces;
            yield return new  WhiteSpaceView
            (
                target.WhiteSpaces
                , configuration
                , isSeparatorRequired,
                lineBreakCount
            );
        }

        [UsedImplicitly]
        static TValue[] T<TValue>(params TValue[] value) => value;
    }
}