using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class SmartFormat : DumpableObject
    {
        internal readonly IGapHandler GapHandler;
        internal readonly ILineLengthLimiter LineLengthLimiter;

        SmartFormat(IGapHandler gapHandler, ILineLengthLimiter lineLengthLimiter)
        {
            GapHandler = gapHandler;
            LineLengthLimiter = lineLengthLimiter;
        }

        internal static string Reformat(SourceSyntax target, SourcePart targetPart)
            => Create().Reformat(target, 0, targetPart);

        static SmartFormat Create()
        {
            var whitespacelessGapHandler = SmartConfiguration.Instance;
            var gapHandler = new KeepCommentConfiguration(whitespacelessGapHandler);
            return new SmartFormat(gapHandler, new LineLengthLimiter(100));
        }

        string Reformat(SourceSyntax target, int indentLevel, SourcePart targetPart)
            => Reformat(null, target, null, null, indentLevel)
                .Combine()
                .Filter(targetPart);


        IEnumerable<Item> Reformat
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int indentLevel
            )
        {
            //Tracer.ConditionalBreak(indentLevel > 0 && rightWhiteSpaces.HasComment());

            if(target == null)
                return Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass, indentLevel);

            if(!IsLineMode(target, indentLevel))
                return UncheckedReformat
                    (leftTokenClass, target, rightWhiteSpaces, rightTokenClass, indentLevel);

            var result = LineModeReformat(target, indentLevel);
            if(result != null)
                return result;

            if(target.IsChain())
                return LineModeReformatOfChain(leftTokenClass, target, indentLevel);

            return UncheckedReformat
                (leftTokenClass, target, rightWhiteSpaces, rightTokenClass, indentLevel);
        }

        IEnumerable<Item> UncheckedReformat
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int indentLevel)
        {
            foreach(var item in Reformat
                (
                    leftTokenClass,
                    target.Left,
                    target.Token.PrecededWith,
                    target.TokenClass,
                    indentLevel
                ))
                yield return item;

            yield return new Item("", target.Token);

            foreach(var item in Reformat
                (
                    target.TokenClass,
                    target.Right,
                    rightWhiteSpaces,
                    rightTokenClass,
                    indentLevel
                ))
                yield return item;
        }

        IEnumerable<Item> LineModeReformat(SourceSyntax target, int indentLevel)
            => MainInformation.Create(target, this)?.LineModeReformat(indentLevel)
                ?? BraceInformation.Create(target, this)?.LineModeReformat(indentLevel);

        IEnumerable<Item> LineModeReformatOfChain
            (ITokenClass leftTokenClass, SourceSyntax target, int indentLevel)
        {
            if(!target.IsChain())
            {
                foreach(var item in Reformat(leftTokenClass, target, null, null, indentLevel))
                    yield return item;
                yield break;
            }

            foreach(var item in LineModeReformatOfChain(leftTokenClass, target.Left, indentLevel))
                yield return item;

            var whiteSpaces = GapHandler.StartGap
                (target.Left == null, indentLevel, target.Token.PrecededWith, target.TokenClass);
            yield return new Item(whiteSpaces, target.Token);

            foreach(var item in Reformat(target.TokenClass, target.Right, null, null, indentLevel))
                yield return item;
        }

        internal IEnumerable<Item> IndentedReformatLineMode
            (
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int indentLevel
            )
            =>
                target == null
                    ? Gap(null, rightWhiteSpaces, rightTokenClass, indentLevel)
                    : ReformatLineMode(target, indentLevel).PrettyLines();

        IEnumerable<IEnumerable<Item>> ReformatLineMode(SourceSyntax target, int indentLevel)
        {
            if(target.TokenClass is List)
                return ReformatListLines(target, indentLevel);

            return new[] {Reformat(null, target, null, null, indentLevel)};
        }

        IEnumerable<IEnumerable<Item>> ReformatListLines(SourceSyntax target, int indentLevel)
        {
            var separator = target.TokenClass;
            do
            {
                yield return
                    Reformat
                        (
                            null,
                            target.Left,
                            target.Token.PrecededWith,
                            target.TokenClass,
                            indentLevel
                        ).plus(new Item("", target.Token));
                target = target.Right;
            } while(target != null && target.TokenClass == separator);

            if(target != null)
                yield return Reformat(null, target, null, null, indentLevel);
        }

        bool IsLineMode(SourceSyntax target, int indentLevel)
        {
            var braceInformation = BraceInformation.Create(target, this);
            if(braceInformation != null)
                return braceInformation.GetIsLineMode(indentLevel);

            return GetLineLengthInformation
                (null, target, null, null, LineLengthLimiter.MaxLineLength, indentLevel) <= 0;
        }

        internal bool Contains(WhiteSpaceToken[] whiteSpaces, ITokenClass tokenClass)
            => GapHandler
                .StartGap(false, 0, whiteSpaces, tokenClass)
                .Contains("\n");

        internal int GetLineLengthInformation
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int lengthRemaining,
            int indentLevel)
        {
            if(target == null)
            {
                var gap = Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass, indentLevel);
                if(gap.Any(item => item.WhiteSpaces.Contains("\n")))
                    return 0;
                return lengthRemaining - gap.Sum(item => item.Length);
            }

            lengthRemaining = GetLineLengthInformation
                (
                    leftTokenClass,
                    target.Left,
                    target.Token.PrecededWith,
                    target.TokenClass,
                    lengthRemaining,
                    indentLevel);
            if(lengthRemaining <= 0)
                return 0;

            lengthRemaining = lengthRemaining - target.Token.Characters.Length;
            if(lengthRemaining <= 0)
                return 0;

            lengthRemaining = GetLineLengthInformation
                (
                    target.TokenClass,
                    target.Right,
                    rightWhiteSpaces,
                    rightTokenClass,
                    lengthRemaining,
                    indentLevel);

            return lengthRemaining;
        }

        IEnumerable<Item> Gap
            (
            ITokenClass leftTokenClass,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int indentLevel)
        {
            if(rightTokenClass == null)
            {
                Tracer.Assert(rightWhiteSpaces == null);
                yield break;
            }

            yield return
                new Item
                    (UnfilteredGap(leftTokenClass, rightWhiteSpaces, rightTokenClass, indentLevel));
        }

        string UnfilteredGap
            (
            ITokenClass leftTokenClass,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int indentLevel)
        {
            if(leftTokenClass == null)
                GapHandler.StartGap(false, indentLevel, rightWhiteSpaces, rightTokenClass);

            return GapHandler.Gap
                (indentLevel, leftTokenClass, rightWhiteSpaces, rightTokenClass);
        }
    }
}