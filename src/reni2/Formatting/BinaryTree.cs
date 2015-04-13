using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;

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
        {}

        BinaryTree(ITreeItem left, TokenItem token, ITreeItem right)
        {
            Left = left;
            Token = token;
            Right = right;
            _lengthCache = new ValueCache<int>(GetLength);
            Tracer.Assert(Token != null);
        }

        string ITreeItem.Reformat(ISubConfiguration configuration)
        {
            NotImplementedMethod(configuration);
            return null;
        }

        IAssessment ITreeItem.Assess(IAssessor assessor)
        {
            var assessment = assessor.Assess(Token);
            if(!assessment.IsMaximal)
                assessment = (Left?.Assess(assessor)).plus(assessment);
            if(!assessment.IsMaximal)
                assessment = assessment.plus(Right?.Assess(assessor));
            return assessment;
        }

        int ITreeItem.Length => _lengthCache.Value;

        int GetLength() => (Left?.Length ?? 0) + Token.Length + (Right?.Length ?? 0);
    }
}