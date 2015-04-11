using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    internal interface IAssessment
    {
        ISubConfiguration Configuration { get; }
        bool IsMaximal { get; }
        IAssessment Combine(IAssessment other);
    }
}