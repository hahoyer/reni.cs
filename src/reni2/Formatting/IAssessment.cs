using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    interface IAssessment
    {
        ISubConfiguration Configuration { get; }
        bool IsMaximal { get; }
        IAssessment Combine(IAssessment other);
    }


    static class Extension
    {
        public static IAssessment plus(this IAssessment left, IAssessment right)
        {
            if(left == null || left == right)
                return right;
            if(right == null)
                return left;
            return left.Combine(right);
        }
    }
}