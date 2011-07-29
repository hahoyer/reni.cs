using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Type
{
    internal interface IFunctionalFeature : IDumpShortProvider
    {
        Result ObtainApplyResult(Category category, Result operationResult, Result argsResult, RefAlignParam refAlignParam);
    }
}