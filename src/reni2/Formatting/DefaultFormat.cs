using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Parser;
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
                return Reformat(target, rightParenthesis) + Format(target.Token);

            var left = target.Left?.Reformat(this);
            var token = Format(target.Token) ?? "";
            var right = target.Right?.Reformat(this);

            return LeftSeparator(target.Left, target.TokenClass).After(left) +
                token +
                RightSeparator(target).Before(right);
        }

        string Reformat(SourceSyntax target, RightParenthesis rightParenthesis)
        {
            Tracer.Assert(target.Right == null);

            var left = target.Left;
            Tracer.Assert(left != null);

            var leftParenthesis = left.TokenClass as LeftParenthesis;
            return leftParenthesis != null
                ? Reformat(leftParenthesis, left, rightParenthesis)
                : left.Reformat(this);
        }

        string Reformat
            (
            LeftParenthesis leftParenthesis,
            SourceSyntax left,
            RightParenthesis rightParenthesis)
        {
            Tracer.Assert(left.Left == null);
            Tracer.Assert(leftParenthesis.Level == rightParenthesis.Level);

            var lefttoken = Format(left.Token);
            var innerTarget = left.Right?.Reformat(this);

            var separator = SeparatorType.Get(leftParenthesis, null)
                .Escalate(() => AssessSeparator(innerTarget));

            return separator.Text +
                lefttoken +
                separator.Before(innerTarget) +
                separator.Text;
        }

        static ISeparatorType AssessSeparator(string target)
            => (target?.Any(item => item == '\n') ?? false)
                ? SeparatorType.Multiline
                : SeparatorType.Contact;

        string Reformat(SourceSyntax target, List token)
        {
            var items = RearrangeAsList(target, token);
            var separator = SeparatorType.Get(token, null);
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
                : SeparatorType.Get(target.TokenClass, target.Right.LeftMostTokenClass);

        static ISeparatorType LeftSeparator(SourceSyntax left, ITokenClass tokenClass)
            => left == null
                ? SeparatorType.None
                : SeparatorType.Get(left.RightMostTokenClass, tokenClass);

        string Format(IToken token)
        {
            if(token == null)
                return "";
            if(token.PrecededWith.OnlyComments().Id() != "")
                NotImplementedMethod(token);
            return token.Id;
        }
    }
}