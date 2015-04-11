using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    interface IConfiguration
    {
        string Reformat(ITreeItem target);
    }

    interface ISubConfiguration
    {
        IConfiguration Parent { get; }
        string Reformat(TokenItem target);
        string ListItemHead { get; }
    }
}