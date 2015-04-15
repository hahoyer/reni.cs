using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Feature;
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

        internal static ISeparatorType BraceSeparator(int level)
            => level < 3
                ? SeparatorType.Contact
                : SeparatorType.Multiline;

        internal static ISeparatorType ListSeparator(int level)
            => level == 0
                ? SeparatorType.Contact
                : SeparatorType.Multiline;

        string IConfiguration.Reformat(SourceSyntax target)
        {
            var listItem = ListItem.CheckedCreate(target);
            if(listItem != null)
                return listItem.Reformat(this);

            var braceItem = BraceItem.CheckedCreate(target);
            if(braceItem != null)
                return braceItem.Reformat(this);

            var left = target.Left?.Reformat(this);
            var leftSeparator = LeftSeparator(target).Text;
            var token = Format(target.Token) ?? "";
            var rightSeparator = RightSeparator(target).Text;
            var right = target.Right?.Reformat(this);

            return left + leftSeparator + token + rightSeparator + right;
        }

        internal static ISeparatorType AssessSeparator(string target)
            => (target?.Any(item => item == '\n') ?? false)
                ? SeparatorType.Multiline
                : SeparatorType.Contact;

        ISeparatorType RightSeparator(SourceSyntax target)
            => target.Right == null
                ? SeparatorType.None
                : Separator(target.TokenClass, target.Right.LeftMostTokenClass);

        ISeparatorType LeftSeparator(SourceSyntax target)
            => target.Left == null
                ? SeparatorType.None
                : Separator(target.Left.RightMostTokenClass, target.TokenClass);

        internal ISeparatorType Separator(ITokenClass left, ITokenClass right)
            => PrettySeparatorType(left, right) ??
                BaseSeparatorType(left, right);

        static ISeparatorType BaseSeparatorType(ITokenClass left, ITokenClass right)
            => ContactClass(left).IsCompatible(ContactClass(right))
                ? SeparatorType.Contact
                : SeparatorType.Close;

        ISeparatorType PrettySeparatorType(ITokenClass left, ITokenClass right)
        {
            if(left is Colon && right is LeftParenthesis)
                NotImplementedMethod(left, right);

            return null;

            if((left is Number && right is TypeOperator) ||
                (left is Text && right is Definable)) {}
            NotImplementedMethod(left, right);

            if(left is RightParenthesis && !(right is List))
                return SeparatorType.Close;

            var leftList = left as List;
            if(leftList != null)
                return leftList.Level > 0 ? SeparatorType.Multiline : SeparatorType.Close;

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

        internal string Format(IToken token)
        {
            if(token == null)
                return "";
            if(token.PrecededWith.OnlyComments().Id() != "")
                NotImplementedMethod(token);
            return token.Id;
        }

        internal static IEnumerable<string> Grouped(IEnumerable<string> items, string separator)
        {
            var lastWasMultilline = false;
            foreach(var item in items)
            {
                var isMultiline = item.Contains(separator);
                if(lastWasMultilline || isMultiline)
                    yield return separator + item;
                else
                    yield return item;
                lastWasMultilline = isMultiline;
            }
        }
    }
}