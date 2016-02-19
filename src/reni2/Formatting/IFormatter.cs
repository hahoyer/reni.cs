using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    public interface IFormatter
    {
        string Reformat(SourceSyntax target, SourcePart part);
    }
}