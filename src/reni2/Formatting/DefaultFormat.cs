using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Parser;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class DefaultFormat : DumpableObject, IConfiguration, IAssessor
    {
        public static readonly DefaultFormat Instance = new DefaultFormat();

        [DisableDump]
        internal readonly ISubConfiguration MultiLineInstance;
        [DisableDump]
        internal readonly ISubConfiguration DefaultInstance;

        DefaultFormat()
        {
            DefaultInstance = new DefaultSubFormat(this);
            MultiLineInstance = new MultiLineFormat(this);
        }

        string IConfiguration.Reformat(ITreeItem target)
            => target?.Reformat(target.Assess(this).Configuration) ?? "";

        IAssessment IAssessor.Assess(TokenItem token)
            => !token.FullText.Contains("\n")
                ? DefaultAssessment.Instance
                : MultiLineAssessment.Instance;

        IAssessment IAssessor.List(List target)
            => target.Level == 0
                ? DefaultAssessment.Instance
                : MultiLineAssessment.Instance;

        IAssessment IAssessor.Length(int target)
            => target < 100
                ? DefaultAssessment.Instance
                : MultiLineAssessment.Instance;

        public IAssessment Brace(int level)
            => level < 3
                ? DefaultAssessment.Instance
                : MultiLineAssessment.Instance;

        string IConfiguration.Reformat(SourceSyntax target)
        {
            var list = target.TokenClass as List;
            if(list != null)
                return ReformatList(target, list);

            var left = target.Left?.Reformat(this);
            var token = Format(target.Token) ?? "";
            var right = target.Right?.Reformat(this);

            return LeftDelimiter(target.Left, target.TokenClass).After(left) +
                token +
                RightDelimiter(target).Before(right);
        }

        string ReformatList(SourceSyntax target, List list)
        {
            var items = RearrangeAsList(target, list);
            IDelimiterType separator = Delimiter(list, null);
            if(separator != DelimiterType.MultiLine)
                return items.Stringify("");

            return Grouped(items, separator.Text).Stringify("");
        }

        static IEnumerable<string> Grouped(IEnumerable<string> items, string separator)
        {
            var lastWasMultilline = false;
            foreach(var item in items)
            {
                var isMultiline = item.Contains(separator);
                if(lastWasMultilline || isMultiline)
                    yield return separator;
                yield return item;
                yield return separator;
                lastWasMultilline = isMultiline;
            }
        }

        IEnumerable<string> RearrangeAsList(SourceSyntax target, List list)
        {
            do
            {
                yield return ListLine(target.Left, target.Token, list);

                target = target.Right;

                if(target == null)
                    yield break;

                if(target.TokenClass != list)
                {
                    yield return ListLine(target, null, list);
                    yield break;
                }
            } while(true);
        }

        string ListLine(SourceSyntax target, IToken token, ITokenClass list)
        {
            var text = target?.Reformat(this);
            return LeftDelimiter(target, list).After(text) +
                (Format(token) ?? "");
        }

        static IDelimiterType RightDelimiter(SourceSyntax target)
            => target.Right == null
                ? DelimiterType.None
                : Delimiter(target.TokenClass, target.Right.LeftMostTokenClass);

        static IDelimiterType LeftDelimiter(SourceSyntax left, ITokenClass tokenClass)
            => left == null
                ? DelimiterType.None
                : Delimiter(left.RightMostTokenClass, tokenClass);

        static IDelimiterType Delimiter(ITokenClass left, ITokenClass right)
            => PrettyDelimiter(left, right) ??
                BaseDelimiterType(left, right);

        static IDelimiterType BaseDelimiterType(ITokenClass left, ITokenClass right)
            => ContactClass(left).IsCompatible(ContactClass(right))
                ? DelimiterType.Contact
                : DelimiterType.Close;

        static IDelimiterType PrettyDelimiter(ITokenClass left, ITokenClass right)
        {
            if(left.Id == "{" && ((right == null) || right.Id != "}"))
                return DelimiterType.Indent;

            if(right != null &&
                (
                    right.Id == "{" || left.Id != "{" && right.Id == "}")
                )
                return DelimiterType.MultiLine;

            if(left is RightParenthesis && !(right is List))
                return DelimiterType.Close;

            var leftList = left as List;
            if(leftList != null)
                return leftList.Level > 0 ? DelimiterType.MultiLine : DelimiterType.Close;

            if(left is Colon)
                return DelimiterType.Close;

            return null;
        }

        static ContactType ContactClass(ITokenClass target)
            => ReniLexer.IsAlphaLike(target.Id) || target is Number
                ? ContactType.AlphaNum
                : target is Text
                    ? ContactType.Text
                    : (ReniLexer.IsSymbolLike(target.Id)
                        ? ContactType.Symbol
                        : ContactType.SingleChar);

        string Format(IToken token)
        {
            if(token == null)
                return "";
            if(token.PrecededWith.OnlyComments().Id() != "")
                NotImplementedMethod(token);
            return token.Id;
        }
    }

    interface IDelimiterType
    {
        string After(string target);
        string Before(string target);
        string Text { get; }
    }

    static class DelimiterType
    {
        internal static readonly IDelimiterType None = new NoneType();
        internal static readonly IDelimiterType Contact = new ConcatType("");
        internal static readonly IDelimiterType Close = new ConcatType(" ");
        internal static readonly IDelimiterType MultiLine = new ConcatType("\n");
        internal static readonly IDelimiterType Indent = new IndentType();

        sealed class IndentType : DumpableObject, IDelimiterType
        {
            string IDelimiterType.After(string target)
            {
                NotImplementedMethod(target);
                return null;
            }

            string IDelimiterType.Before(string target) => ("\n" + target).Indent();
            string IDelimiterType.Text => "\n";
        }

        sealed class NoneType : DumpableObject, IDelimiterType
        {
            string IDelimiterType.After(string target)
            {
                Tracer.Assert(target == null);
                return "";
            }

            string IDelimiterType.Before(string target)
            {
                Tracer.Assert(target == null);
                return "";
            }

            string IDelimiterType.Text => null;
        }

        sealed class ConcatType : DumpableObject, IDelimiterType
        {
            readonly string _delimiter;
            internal ConcatType(string delimiter) { _delimiter = delimiter; }

            string IDelimiterType.After(string target) => target + _delimiter;
            string IDelimiterType.Before(string target) => _delimiter + target;
            string IDelimiterType.Text => _delimiter;
        }
    }

    sealed class ContactType
    {
        internal static readonly ContactType AlphaNum = new ContactType();
        internal static readonly ContactType Symbol = new ContactType();
        internal static readonly ContactType Text = new ContactType();
        internal static readonly ContactType SingleChar = new ContactType();

        public bool IsCompatible(ContactType other)
        {
            if(this == SingleChar)
                return true;
            if(other == SingleChar)
                return true;
            return this != other;
        }
    }
}