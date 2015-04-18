using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class ListTree : DumpableObject, ITreeItem
    {
        public static readonly ITreeItemFactory FactoryInstance = new Factory();

        readonly List _tokenClass;
        internal readonly Item[] Items;

        internal ListTree(List tokenClass, Item[] items)
        {
            _tokenClass = tokenClass;
            Items = items;
        }

        internal sealed class Item : DumpableObject
        {
            readonly ITreeItem _left;
            readonly TokenItem _token;

            internal Item(ITreeItem left, TokenItem token)
            {
                _left = left;
                _token = token;
            }

            public ITokenClass LeftMostTokenClass => _left?.LeftMostTokenClass ?? _token?.Class;
            public ITokenClass RightMostTokenClass => _token?.Class ?? _left?.RightMostTokenClass;

            internal string Reformat(IConfiguration configuration)
                => configuration.Reformat(_left) + (_token?.Id ?? "");

            internal int UseLength(int length)
                => (_left?.UseLength(length) ?? length) - (_token?.Length ?? 0);
        }

        sealed class Factory : DumpableObject, ITreeItemFactory
        {
            ITreeItem ITreeItemFactory.Create(ITreeItem left, TokenItem token, ITreeItem right)
            {
                var listItem = new Item(left, token);
                var level = (List) token.Class;
                return right?.List(level, listItem) ?? new ListTree(level, new[] {listItem});
            }
        }

        ITreeItem ITreeItem.List(List level, Item left)
            => new ListTree(level, left.plus(Items));

        string ITreeItem.Reformat(IConfiguration configuration, ISeparatorType separator)
            => configuration.Reformat(this, separator);

        int ITreeItem.UseLength(int length)
        {
            var result = length - Items.Length;
            foreach(var item in Items.Where(item => result > 0))
                result = item.UseLength(result);
            return result;
        }

        ITokenClass ITreeItem.RightMostTokenClass
            => Items.LastOrDefault()?.RightMostTokenClass ?? _tokenClass;

        ITokenClass ITreeItem.LeftMostTokenClass
            => Items.FirstOrDefault()?.LeftMostTokenClass ?? _tokenClass;

        string ITreeItem.DefaultReformat => DefaultFormat.Instance.Reformat(this);
    }
}