using System;
using System.Collections.Generic;
using System.Linq;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    internal interface IAssessor
    {
        IAssessment Assess(TokenItem token);
        IAssessment List(List target);
        IAssessment Length(int target);
        IAssessment Brace(int level);
    }
}