using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    sealed class LineLengthLimiter : ILineLengthLimiter
    {
        readonly int _maxLineLength;
        public LineLengthLimiter(int maxLineLength) { _maxLineLength = maxLineLength; }
        int ILineLengthLimiter.MaxLineLength => _maxLineLength;
    }
}