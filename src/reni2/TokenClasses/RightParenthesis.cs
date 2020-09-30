using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class RightParenthesis : RightParenthesisBase,
        IValueProvider,
        IDefaultScopeProvider,
        IBracketMatch<Syntax>
    {
        sealed class Matched : DumpableObject,
            IParserTokenType<Syntax>,
            ITokenClass,
            IValueProvider
        {
            static string Id => "()";

            Result<Value> IValueProvider.Get(Syntax syntax)
            {
                Tracer.Assert(syntax.Left != null);
                Tracer.Assert(syntax.Right != null);
                var leftValue = syntax.Left.Value;
                var rightValue = syntax.Right.Value;
                return ExpressionSyntax.Create(syntax, leftValue.Target, null, rightValue.Target)
                    .With(rightValue.Issues.plus(leftValue.Issues));
            }

            Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
                => right == null ? left : Syntax.Create(left, this, token, right);

            string IParserTokenType<Syntax>.PrioTableId => Id;
            string ITokenClass.Id => Id;
        }

        public RightParenthesis(int level)
            : base(level) { }

        [Obsolete("",true)]
        Result<Value> IValueProvider.Get(Syntax syntax)
        {
            var result = syntax.Left.GetBracketKernel(Level, syntax);
            var target = result.Target?.Value ?? new EmptyList(syntax);

            if(result.Issues.Any())
                return target.With(result.Issues);

            return result.Target == null ? new EmptyList(syntax) : result.Target.Value;
        }

        bool IDefaultScopeProvider.MeansPublic => Level == 3;

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();
    }
}