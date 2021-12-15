using System.Collections.Generic;
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
            var isSeparatorRequired = target.IsSeparatorRequired(configuration.EmptyLineLimit!= 0);
            yield return new  WhiteSpaces
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