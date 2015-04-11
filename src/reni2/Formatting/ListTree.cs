using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class ListTree : DumpableObject, ITreeItem
    {
        public static readonly ITreeItemFactory FactoryInstance = new Factory();

        internal readonly List TokenClass;
        internal readonly ListItem[] Items;

        internal ListTree(List tokenClass, ListItem[] items)
        {
            TokenClass = tokenClass;
            Items = items;
        }

        sealed class Factory : DumpableObject, ITreeItemFactory
        {
            ITreeItem ITreeItemFactory.Create(ITreeItem left, TokenItem token, ITreeItem right)
            {
                var listItem = new ListItem(left, token);
                var level = (List) token.Class;
                return right?.List(level, listItem) ?? new ListTree(level, new[] {listItem});
            }
        }

        ITreeItem ITreeItem.List(List level, ListItem left)
            => new ListTree(level, left.plus(Items));

        IAssessment ITreeItem.Assess(IAssessor assessor)
        {
            var result = assessor.List(TokenClass);

            foreach(var item in Items)
            {
                if(result.IsMaximal)
                    return result;
                result = result.Combine(item.Assess(assessor));
            }

            return result;
        }

        int ITreeItem.Length => Items.Sum(item => item.Length);

        string ITreeItem.Reformat(ISubConfiguration configuration)
        {
            var lines = Items.Select(item => item.Reformat(configuration));
            return PrettyLines(lines).Stringify("");
        }

        static IEnumerable<string> PrettyLines(IEnumerable<string> lines)
        {
            var wasMultiline = false;
            var addNewLine = false;

            foreach(var line in lines)
            {
                var isMultiline = line.Where(item => item == '\n').Skip(1).Any();
                if(addNewLine && (wasMultiline || isMultiline))
                    yield return "\n";
                yield return line;
                wasMultiline = isMultiline;
                addNewLine = true;
            }
        }
    }
}