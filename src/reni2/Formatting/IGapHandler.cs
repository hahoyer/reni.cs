using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    internal interface IGapHandler
    {
        string Gap(ITokenClass left, ITokenClass rightTokenClass);
        string StartGap(ITokenClass right);
    }
}