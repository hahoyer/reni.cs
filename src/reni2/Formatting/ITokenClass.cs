using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    interface ITokenClass
    {
        ITreeItemFactory TreeItemFactory { get; }
        string Id { get; }
    }

    interface ITreeItemFactory
    {
        ITreeItem Create(ITreeItem left, TokenItem token, ITreeItem right);
    }
}