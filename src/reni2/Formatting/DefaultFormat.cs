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
                return Reformat(target, list);

            var rightParenthesis = target.TokenClass as RightParenthesis;
            if(rightParenthesis != null)
                return Reformat(target, rightParenthesis);

            var left = target.Left?.Reformat(this);
            var token = Format(target.Token) ?? "";
            var right = target.Right?.Reformat(this);

            return LeftSeparator(target.Left, target.TokenClass).After(left) +
                token +
                RightSeparator(target).Before(right);
        }

        string Reformat(SourceSyntax target, RightParenthesis rightParenthesis)
        {
            var left = target.Left;
            Tracer.Assert(left != null);
            Tracer.Assert(target.Right == null);
            Tracer.Assert(left.Left == null);
            var leftParenthesis = ((LeftParenthesis) left.TokenClass);
            Tracer.Assert(leftParenthesis.Level == rightParenthesis.Level);

            var lefttoken = Format(left.Token);
            var rightToken = Format(target.Token);
            var innerTarget = left.Right?.Reformat(this);

            var separator = Separator(leftParenthesis, null)
                .Escalate(() => AssessSeparator(innerTarget));

            return separator.Text + 
                lefttoken +
                separator.Before(innerTarget) +
                separator.Text +
                rightToken;
        }

        static ISeparatorType AssessSeparator(string target)
            => (target?.Any(item => item == '\n') ?? false)
                ? SeparatorType.Multiline
                : SeparatorType.Contact;

        string Reformat(SourceSyntax target, List token)
        {
            var items = RearrangeAsList(target, token);
            var separator = Separator(token, null);
            return separator
                .Grouped(items)
                .Stringify(separator.Text);
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
            return LeftSeparator(target, list).After(text) + (Format(token) ?? "");
        }

        static ISeparatorType RightSeparator(SourceSyntax target)
            => target.Right == null
                ? SeparatorType.None
                : Separator(target.TokenClass, target.Right.LeftMostTokenClass);

        static ISeparatorType LeftSeparator(SourceSyntax left, ITokenClass tokenClass)
            => left == null
                ? SeparatorType.None
                : Separator(left.RightMostTokenClass, tokenClass);

        static ISeparatorType Separator(ITokenClass left, ITokenClass right)
            => PrettySeparatorType(left, right) ??
                BaseSeparatorType(left, right);

        static ISeparatorType BaseSeparatorType(ITokenClass left, ITokenClass right)
            => ContactClass(left).IsCompatible(ContactClass(right))
                ? SeparatorType.Contact
                : SeparatorType.Close;

        static ISeparatorType PrettySeparatorType(ITokenClass left, ITokenClass right)
        {
            if(left is RightParenthesis && !(right is List))
                return SeparatorType.Close;

            var leftList = left as List;
            if(leftList != null)
                return leftList.Level > 0 ? SeparatorType.ClusteredMultiLine : SeparatorType.Close;

            if(left is Colon)
                return SeparatorType.Close;

            return null;
        }

        static ContactType ContactClass(ITokenClass target)
            => target == null
                ? ContactType.Compatible
                : ReniLexer.IsAlphaLike(target.Id) || target is Number
                    ? ContactType.AlphaNum
                    : target is Text
                        ? ContactType.Text
                        : (ReniLexer.IsSymbolLike(target.Id)
                            ? ContactType.Symbol
                            : ContactType.Compatible);

        string Format(IToken token)
        {
            if(token == null)
                return "";
            if(token.PrecededWith.OnlyComments().Id() != "")
                NotImplementedMethod(token);
            return token.Id;
        }
    }

    sealed class ContactType
    {
        internal static readonly ContactType AlphaNum = new ContactType();
        internal static readonly ContactType Symbol = new ContactType();
        internal static readonly ContactType Text = new ContactType();
        internal static readonly ContactType Compatible = new ContactType();

        public bool IsCompatible(ContactType other)
        {
            if(this == Compatible)
                return true;
            if(other == Compatible)
                return true;
            return this != other;
        }
    }
}