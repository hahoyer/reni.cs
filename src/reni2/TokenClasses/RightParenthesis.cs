﻿using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class RightParenthesis
        : RightParenthesisBase
            , IValueProvider
            , IDefaultScopeProvider
            , IBracketMatch<BinaryTree>
            , ISyntaxScope
            , ISyntaxFactoryToken
    {
        sealed class Matched : DumpableObject, IParserTokenType<BinaryTree>, ITokenClass, IValueProvider
        {
            static string Id => "()";

            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
                => right == null? left : BinaryTree.Create(left, this, token, right);

            string IParserTokenType<BinaryTree>.PrioTableId => Id;
            string ITokenClass.Id => Id;

            Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
            {
                Tracer.Assert(binaryTree.Left != null);
                Tracer.Assert(binaryTree.Right != null);
                var leftValue = binaryTree.Left.Syntax(scope);
                var rightValue = binaryTree.Right.Syntax(scope);
                return ExpressionSyntax.Create(binaryTree, leftValue.Target, null, rightValue.Target)
                    .With(rightValue.Issues.plus(leftValue.Issues));
            }
        }

        public RightParenthesis(int level)
            : base(level) { }

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();

        bool IDefaultScopeProvider.MeansPublic => Level == 3;
        ISyntaxFactory ISyntaxFactoryToken.Provider => SyntaxFactory.Bracket;
        IDefaultScopeProvider ISyntaxScope.DefaultScopeProvider => this;
        bool ISyntaxScope.IsDeclarationPart => false;

        [Obsolete("", true)]
        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            var result = binaryTree.Left.GetBracketKernel(Level, binaryTree);
            var target = result.Target?.Syntax(this) ?? new EmptyList(binaryTree);

            if(result.Issues.Any())
                return target.With(result.Issues);

            return result.Target == null? new EmptyList(binaryTree) : result.Target.Syntax(this);
        }
    }
}