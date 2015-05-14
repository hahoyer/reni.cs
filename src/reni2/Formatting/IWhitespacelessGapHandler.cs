using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    internal interface IWhitespacelessGapHandler
    {
        string Gap(int indentLevel, ITokenClass left, ITokenClass rightTokenClass);
        string StartGap(int indentLevel, ITokenClass right);
    }
}