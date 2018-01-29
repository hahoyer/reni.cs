using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Stx.CodeItems;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Reassign : TokenClass
    {
        public const string TokenId = ":=";

        static Result ValidateDestination
            (Context context, Syntax syntax, SourcePart position, DataType inferredDataType)
            => syntax == null
                ? IssueId.ReassignDestinationMissing.At(position)
                : syntax.GetResult(context.ReassignDestination(inferredDataType));

        static Result ValidateValue(Context context, Syntax syntax, SourcePart position)
            => syntax == null
                ? IssueId.ReassignValueMissing.At(position)
                : syntax.GetResult(context.ReassignValue);

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult
            (Context context, Syntax left, IToken token, Syntax right)
        {
            var value = ValidateValue(context, right, token.Characters);
            var destination = ValidateDestination(context, left, token.Characters, value.DataType);

            return
                new Result
                (
                    token.Characters,
                    getCodeItems: () =>
                        CodeItem
                            .Combine
                            (
                                destination.CodeItems,
                                CodeItem.CreateSourceHint(token),
                                value.CodeItems,
                                CodeItem.CreateReassign(value.ByteSize)
                            )
                            .ToArray()
                );
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class Colon : TokenClass
    {
        public const string TokenId = ":";

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult
            (Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}