using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace Reni.Formatting
{
    interface IConfiguration
    {
        string Reformat(ITreeItem target);
        string Reformat(ListTree target, ISeparatorType separator);
        string Reformat(BinaryTree target);
    }
}