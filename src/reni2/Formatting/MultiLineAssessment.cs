using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class MultiLineAssessment : DumpableObject, IAssessment
    {
        public static readonly IAssessment Instance = new MultiLineAssessment();
        MultiLineAssessment() {}
        bool IAssessment.IsMaximal => true;
        IAssessment IAssessment.Combine(IAssessment other) => this;
        ISubConfiguration IAssessment.Configuration => DefaultFormat.Instance.MultiLineInstance;
    }
}