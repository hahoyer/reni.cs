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
        , IValuesScope
    {
        sealed class Matched : DumpableObject,
            IParserTokenType<Syntax>,
            ITokenClass,
            IValueProvider
        {
            static string Id => "()";

            Result<Value> IValueProvider.Get(Syntax syntax, IValuesScope scope)
            {
                Tracer.Assert(syntax.Left != null);
                Tracer.Assert(syntax.Right != null);
                var leftValue = syntax.Left.Value(scope);
                var rightValue = syntax.Right.Value(scope);
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
        Result<Value> IValueProvider.Get(Syntax syntax, IValuesScope scope)
        {
            var result = syntax.Left.GetBracketKernel(Level, syntax);
            var target = result.Target?.Value(this) ?? new EmptyList(syntax);

            if(result.Issues.Any())
                return target.With(result.Issues);

            return result.Target == null ? new EmptyList(syntax) : result.Target.Value(this);
        }

        bool IDefaultScopeProvider.MeansPublic => Level == 3;
        IDefaultScopeProvider IValuesScope.DefaultScopeProvider => this;
        bool IValuesScope.IsDeclarationPart => false;
        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();
    }
}