using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public sealed class Edit : DumpableObject
    {
        public SourcePart Location;
        public string NewText;
        public Edit() {StopByObjectIds(689);}
    }

}