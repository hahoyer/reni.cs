using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    static class Extension
    {
        internal static string Parenthesis
            (
            SourceSyntax target,
            IEnumerable<WhiteSpaceToken> tail,
            IConfiguration configuration
            )
        {
            Tracer.Assert(target.Right == null);
            Tracer.Assert(target.Left != null);

            var subTarget = target.Left;

            Tracer.Assert
                (
                    ((LeftParenthesis) subTarget.TokenClass).Level
                        == ((LeftParenthesis) target.TokenClass).Level);
            Tracer.Assert(subTarget.Left == null);

            var leftTail =
                (subTarget.Right?.Token.PrecededWith ?? target.Token.PrecededWith).FilterLeftPart();

            var content = subTarget.Right?.Reformat
                (target.Token.PrecededWith.FilterLeftPart(), configuration);

            if(!configuration.IsMultiline
                && (leftTail.HasLines() || (content?.Contains("\n") ?? false)))
            {
                configuration = new MultilineConfiguraton(configuration);
                content = subTarget.Right?.Reformat
                    (target.Token.PrecededWith.FilterLeftPart(), configuration);
            }

            return configuration.Parenthesis
                (
                    subTarget.Token.PrecededWith.FilterRightPart(),
                    subTarget.Token.Characters.Id,
                    leftTail,
                    content,
                    target.Token.PrecededWith.FilterRightPart(),
                    target.Token.Characters.Id,
                    tail
                );
        }

        internal static string TokenClass
            (SourceSyntax target, IEnumerable<WhiteSpaceToken> tail, IConfiguration configuration)
            => configuration.Default
                (
                    target.Left?.Reformat(target.Token.PrecededWith.FilterLeftPart(), configuration),
                    target.Token.PrecededWith.FilterRightPart(),
                    target.Token.Characters.Id,
                    target.Right?.Token.PrecededWith.FilterRightPart() ?? tail,
                    target.Right?.Reformat(tail, configuration)
                );

        internal static string EndToken
            (SourceSyntax target, IEnumerable<WhiteSpaceToken> tail, IConfiguration configuration)
        {
            Tracer.Assert(target.Left != null && target.Right == null);
            return target.Left.Reformat(tail, configuration);
        }

        internal static string List(SourceSyntax target, IConfiguration configuration)
        {
            var array = Flatten(target);
            var parts = array.Select
                (
                    item =>
                        item
                            ?.Left
                            ?.Reformat
                            (item.Token.PrecededWith.FilterLeftPart(), configuration.SingleLine)
                            ?? ""
                );

            var headers =
                array.Select(item => (item?.Token.PrecededWith).OnlyComments().ToArray())
                    .ToArray();
            var tails =
                array.Select
                    (
                        item =>
                            (item?.Right?.LeadingWhiteSpaceTokens).OnlyComments().ToArray()
                    )
                    .ToArray()
                ;

            var list = (List) target.TokenClass;
            var isMultiLine = configuration.IsMultiline ||
                parts.Any(item => item.Contains("\n")) ||
                headers.Any(item => item.HasLines()) ||
                tails.Any(item => item.HasLines()) ||
                (
                    configuration.MaxListItemLength(list) != null &&
                        parts.Any
                            (item => item.Length > configuration.MaxListItemLength(list))
                    )
                ||
                (
                    configuration.MaxListLength(list) != null &&
                        parts.Sum(item => item.Length + 2) - 2
                            > configuration.MaxListLength(list)
                    );

            var delimiter = target.Token.Characters.Id + (isMultiLine ? "\n" : " ");

            Tracer.Assert(headers.Length == tails.Length);
            Tracer.Assert(headers.Length == parts.Count());
            Tracer.Assert(!headers.Any(item => item?.Any() ?? false));
            Tracer.Assert(!tails.Any(item => item?.Any() ?? false));

            return parts
                .Select(item => item)
                .Stringify(delimiter);
        }
        internal static IEnumerable<SourceSyntax> Flatten(SourceSyntax target)
        {
            var list = target.TokenClass;
            var current = target;
            do
            {
                Tracer.Assert(current.Left == null || current.Left.TokenClass != list);
                yield return current;
                current = current.Right;
            } while(current != null && current.TokenClass == list);
            yield return current;
        }
        internal static string Exclamation
            (
            SourceSyntax target,
            IEnumerable<WhiteSpaceToken> followedBy,
            IConfiguration configuration)
        {
            Tracer.Assert(target.Left == null);
            Tracer.Assert(target.Right != null);

            var right = target.Right.Reformat(followedBy, configuration);
            return configuration.Exclamation(target.Token, right);
        }
    }
}