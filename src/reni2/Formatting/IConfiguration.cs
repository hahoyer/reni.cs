using System;
using System.Collections.Generic;
using System.Linq;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    interface IConfiguration
    {
        string Reformat(ITreeItem target);
        string Reformat(SourceSyntax target);
    }

    interface ISubConfiguration
    {
        IConfiguration Parent { get; }
        string Reformat(TokenItem target);
        string ListItemHead { get; }
    }
}