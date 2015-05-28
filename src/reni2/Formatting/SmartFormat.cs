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
        internal readonly int? MaxLineLength;
        internal readonly int? EmptyLineLimit;

        internal interface IMode
        {
            int LeadingLineBreaks { get; }
        }

        SmartFormat(int maxLineLength, int emptyLineLimit)
        {
            MaxLineLength = maxLineLength;
            EmptyLineLimit = emptyLineLimit;
        }

        internal static string Reformat(SourceSyntax target, SourcePart targetPart)
            => Frame.CreateFrame(target, Create())
                .GetItems()
                .Filter(targetPart);

        static SmartFormat Create() => new SmartFormat(100, 0);

        internal bool IsRelevantLineBreak(int emptyLines, ITokenClass tokenClass)
        {
            if(EmptyLineLimit == null)
                return true;
            if(tokenClass is RightParenthesis)
                return false;
            if(tokenClass is LeftParenthesis)
                return false;
            if(tokenClass is List)
                return false;

            return emptyLines < EmptyLineLimit.Value;
        }

        public interface IRightBraceMode
        {
            IMode Mode { get; }
        }

        public interface IBraceMode
        {
            IRightBraceMode RightMode { get; }
            IMode Mode { get; }
        }

        internal interface IListMode
        {
            IBraceMode BraceMode { get; }
            IMode Combine(IMode previous, IMode current);
        }

        internal interface IListItemMode {}
    }
}