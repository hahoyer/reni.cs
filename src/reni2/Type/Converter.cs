using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Type
{
    interface IConverter
    {
        TypeBase TargetType { get; }
        Result Result(Category category);
    }
}