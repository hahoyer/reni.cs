using System;
using System.Linq;
using System.Collections.Generic;

namespace Reni.Type
{
    internal interface IFunctionalFeature : IDumpShortProvider
    {
        Result Apply(Category category, Result objectResult, Result argsResult);
    }
}