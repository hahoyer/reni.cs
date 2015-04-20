using System;
using System.Collections.Generic;
using System.Linq;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    interface ITreeItem
    {
        string Reformat(IConfiguration configuration);
        ITreeItem List(List level, ListTree.Item left);
        int UseLength(int length);
        ITokenClass RightMostTokenClass { get; }
        ITokenClass LeftMostTokenClass { get; }
        string DefaultReformat { get; }
    }
}