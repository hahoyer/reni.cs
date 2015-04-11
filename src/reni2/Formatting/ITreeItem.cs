using System;
using System.Collections.Generic;
using System.Linq;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    interface ITreeItem
    {
        string Reformat(ISubConfiguration configuration);
        ITreeItem List(List level, ListItem left);
        IAssessment Assess(IAssessor configuration);
        int Length{ get; }
    }
}