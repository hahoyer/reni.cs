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
        IBracketMatch<BinaryTree>
        , IValuesScope
    {
        sealed class Matched : DumpableObject,
            IParserTokenType<BinaryTree>,
            ITokenClass,
            IValueProvider
        {
            static string Id => "()";

            Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, IValuesScope scope)
            {
                Tracer.Assert(binaryTree.Left != null);
                Tracer.Assert(binaryTree.Right != null);
                var leftValue = binaryTree.Left.Syntax(scope);
                var rightValue = binaryTree.Right.Syntax(scope);
                return ExpressionSyntax.Create(binaryTree, leftValue.Target, null, rightValue.Target)
                    .With(rightValue.Issues.plus(leftValue.Issues));
            }

            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
                => right == null ? left : BinaryTree.Create(left, this, token, right);

            string IParserTokenType<BinaryTree>.PrioTableId => Id;
            string ITokenClass.Id => Id;
        }

        public RightParenthesis(int level)
            : base(level) { }

        [Obsolete("",true)]
        Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, IValuesScope scope)
        {
            var result = binaryTree.Left.GetBracketKernel(Level, binaryTree);
            var target = result.Target?.Syntax(this) ?? new EmptyList(binaryTree);

            if(result.Issues.Any())
                return target.With(result.Issues);

            return result.Target == null ? new EmptyList(binaryTree) : result.Target.Syntax(this);
        }

        bool IDefaultScopeProvider.MeansPublic => Level == 3;
        IDefaultScopeProvider IValuesScope.DefaultScopeProvider => this;
        bool IValuesScope.IsDeclarationPart => false;
        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();
    }
}