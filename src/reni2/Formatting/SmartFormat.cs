using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class SmartFormat : DumpableObject
    {
        internal readonly int MaxLineLength;
        internal readonly int? EmptyLineLimit;

        internal interface IMode {}

        SmartFormat(int maxLineLength, int emptyLineLimit)
        {
            MaxLineLength = maxLineLength;
            EmptyLineLimit = emptyLineLimit;
        }

        internal static string Reformat(SourceSyntax target, SourcePart targetPart)
            => new Frame(Create(), target).Items
                .Combine()
                .Filter(targetPart);

        static SmartFormat Create() => new SmartFormat(100, 1);

        internal bool IsRelevantLineBreak(int emptyLines, ITokenClass tokenClass)
        {
            if(EmptyLineLimit == null)
                return true;
            if(tokenClass is RightParenthesis)
                return false;
            if (tokenClass is LeftParenthesis)
                return false;

            return emptyLines <= EmptyLineLimit.Value;
        }
    }
}