using System;
using System.Collections.Generic;
using System.Linq;

namespace ReniUI.Formatting
{
    public sealed class Configuration
    {
        public int? EmptyLineLimit;
        public int? MaxLineLength;
        public int IndentCount = 4;
        public bool SpaceBeforeListItem = false;
        public bool SpaceAfterListItem = true;
    }
}