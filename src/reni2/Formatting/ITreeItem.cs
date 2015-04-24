using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    interface ITreeItem
    {
        string Reformat(ISubConfiguration configuration);
        IAssessment Assess(IAssessor configuration);
        int Length { get; }
    }
}