using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class SmartFormat : DumpableObject
    {
        readonly IGapHandler _gapHandler;
        readonly IGapHandlerWithWhiteSpaces _gapHandlerWithWhiteSpaces;
        internal readonly ILineLengthLimiter LineLengthLimiter;

        SmartFormat
            (
            IGapHandler gapHandler,
            IGapHandlerWithWhiteSpaces gapHandlerWithWhiteSpaces,
            ILineLengthLimiter lineLengthLimiter
            )
        {
            _gapHandler = gapHandler;
            _gapHandlerWithWhiteSpaces = gapHandlerWithWhiteSpaces;
            LineLengthLimiter = lineLengthLimiter;
        }

        internal static string Reformat(SourceSyntax target, SourcePart targetPart)
        {
            var smartFormat = new SmartFormat
                (
                SmartConfiguration.Instance,
                new IgnoreWhiteSpaceConfiguration(SmartConfiguration.Instance),
                new LineLengthLimiter(100)
                );
            return smartFormat.Reformat(target, 0, targetPart);
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
            if(target == null)
                return Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass);

            if(IsLineMode(target))
            {
                var result = LineModeReformat(target, indentLevel);
                if(result != null)
                    return result;

                if(target.IsChain())
                    return LineModeReformatOfChain(leftTokenClass, target, indentLevel);
            }

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

            yield return new Item(target.Token, "");

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
        {
            var main = MainInformation.Create(target, this);
            if(main != null)
                return main.LineModeReformat(indentLevel);

            var brace = BraceInformation.Create(target, this);
            if(brace != null)
                return brace.LineModeReformat(indentLevel);

            return null;
        }

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


            yield return
                new Item
                    (
                    target.Token,
                    (target.Left == null ? "" : "\n" + " ".Repeat(indentLevel * 4))
                        + _gapHandlerWithWhiteSpaces.StartGap
                            (target.Token.PrecededWith, target.TokenClass));

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
        {
            if(target == null)
                return Gap(null, rightWhiteSpaces, rightTokenClass);

            var indent = " ".Repeat(indentLevel * 4);

            return
                ReformatLineMode(target, indentLevel)
                    .Select(item => new Item(null, indent).plus(item))
                    .PrettyLines();
        }

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
                        ).plus(new Item(target.Token, ""));
                target = target.Right;
            } while(target != null && target.TokenClass == separator);

            if(target != null)
                yield return Reformat(null, target, null, null, indentLevel);
        }

        bool IsLineMode(SourceSyntax target)
        {
            var braceInformation = BraceInformation.Create(target, this);
            if(braceInformation != null)
                return braceInformation.IsLineMode;

            return GetLineLengthInformation
                (null, target, null, null, LineLengthLimiter.MaxLineLength) <= 0;
        }

        internal bool Contains(WhiteSpaceToken[] whiteSpaces, ITokenClass tokenClass)
            => _gapHandlerWithWhiteSpaces
                .StartGap(whiteSpaces, tokenClass)
                .Contains("\n");

        internal int GetLineLengthInformation
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int lengthRemaining)
        {
            if(target == null)
            {
                var gap = Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass);
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
                    lengthRemaining
                );
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
                    lengthRemaining
                );

            return lengthRemaining;
        }

        IEnumerable<Item> Gap
            (
            ITokenClass leftTokenClass,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass
            )
        {
            if(rightTokenClass == null)
            {
                Tracer.Assert(rightWhiteSpaces == null);
                yield break;
            }

            yield return
                new Item(null, UnfilteredGap(leftTokenClass, rightWhiteSpaces, rightTokenClass));
        }

        string UnfilteredGap
            (
            ITokenClass leftTokenClass,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass)
        {
            if(leftTokenClass == null)
                return !rightWhiteSpaces.Any()
                    ? _gapHandler.StartGap(rightTokenClass)
                    : _gapHandlerWithWhiteSpaces.StartGap(rightWhiteSpaces, rightTokenClass);

            return !rightWhiteSpaces.Any()
                ? _gapHandler.Gap(leftTokenClass, rightTokenClass)
                : _gapHandlerWithWhiteSpaces.Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass);
        }
    }
}