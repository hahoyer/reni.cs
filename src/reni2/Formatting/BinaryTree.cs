using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class BinaryTree : DumpableObject, ITreeItem
    {
        internal readonly ITreeItem Left;
        internal readonly TokenItem Token;
        internal readonly ITreeItem Right;

        readonly ValueCache<int> _lengthCache;

        public static readonly ITreeItemFactory FactoryInstance = new Factory();

        sealed class Factory : ITreeItemFactory
        {
            ITreeItem ITreeItemFactory.Create(ITreeItem left, TokenItem token, ITreeItem right)
                => new BinaryTree(left, token, right);
        }

        BinaryTree(ITreeItem left, TokenItem token, ITreeItem right)
        {
            Left = left;
            Token = token;
            Right = right;
            _lengthCache = new ValueCache<int>(GetLength);
            Tracer.Assert(Token != null);
        }

        ITreeItem ITreeItem.List(List level, ListItem left)
            => new ListTree(level, new[] {left, new ListItem(this, null)});

        IAssessment ITreeItem.Assess(IAssessor assessor)
        {
            var assessment = assessor.Assess(Token);
            if(!assessment.IsMaximal)
                assessment = assessment.Combine(Left?.Assess(assessor));
            if(!assessment.IsMaximal)
                assessment = assessment.Combine(Right?.Assess(assessor));
            return assessment;
        }

        string ITreeItem.Reformat(ISubConfiguration configuration)
        {
            var left = configuration.Parent.Reformat(Left);
            var token = configuration.Reformat(Token);
            var right = configuration.Parent.Reformat(Right);
            return left + token + right;
        }

        int ITreeItem.Length => _lengthCache.Value;

        int GetLength() => (Left?.Length ?? 0) + Token.Length + (Right?.Length ?? 0);
    }
}