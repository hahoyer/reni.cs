using System;
using System.Collections.Generic;
using System.Linq;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    interface ITreeItem
    {
        string Reformat(IConfiguration configuration, ISeparatorType separator);
        ITreeItem List(List level, ListTree.Item left);
        int Length { get; }
        int UseLength(int length);
        ITokenClass RightMostTokenClass { get; }
        ITokenClass LeftMostTokenClass { get; }
    }
}