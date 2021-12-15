using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    class SpacesGroup : DumpableObject
    {

        SpacesGroup(WhiteSpaceItem item)
        {
            Item =item
        }

        public SourcePart SourcePart
        {
            get { throw new System.NotImplementedException(); }
        }

        internal static SpacesGroup[] Create(WhiteSpaceItem[] items)
            => items != null
                ? items.Select(item => new SpacesGroup(item))
                    .ToArray()
                : new SpacesGroup[0];

        static bool TailCondition(WhiteSpaceItem item) => item.Type is not ISpace;
    }
}