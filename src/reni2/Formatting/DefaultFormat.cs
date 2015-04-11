using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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
    }
}