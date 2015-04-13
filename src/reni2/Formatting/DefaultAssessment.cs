using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class DefaultAssessment : DumpableObject, IAssessment
    {
        public static readonly IAssessment Instance = new DefaultAssessment();
        ISubConfiguration IAssessment.Configuration => DefaultFormat.Instance.DefaultInstance;
        bool IAssessment.IsMaximal => false;

        IAssessment IAssessment.Combine(IAssessment other)
        {
            if(other.IsMaximal)
                return other;

            NotImplementedMethod(other);
            return null;
        }
    }
}