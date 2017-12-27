using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;


namespace ReniUI.Formatting
{
    sealed class ListStructure : Structure
    {
        IStructure[] BodyItemsValue;
        FormatterTokenGroup[] ListItemsValue;

        public ListStructure(Syntax syntax, StructFormatter parent, bool isLineBreakRequired)
            : base(syntax, parent)
        {
            IsLineBreakRequired = isLineBreakRequired;
        }

        new readonly bool IsLineBreakRequired;

        IStructure[] BodyItems
            => BodyItemsValue
               ?? (BodyItemsValue = GetBodyItems().Select(i => i.CreateListItemStruct(Parent)).ToArray());

        FormatterTokenGroup[] ListItems => ListItemsValue ?? (ListItemsValue = GetListItems().ToArray());

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
            => BodyItems
                .SelectMany((item, index) => GetSourcePartEdits(targetPart, item, index - 1, exlucdePrefix));

        IEnumerable<ISourcePartEdit> GetSourcePartEdits
            (SourcePart targetPart, IStructure item, int index, bool? exlucdePrefix)
        {
            var result = item.GetSourcePartEdits(targetPart, index < 0 ? exlucdePrefix : true);
            return index >= 0
                ? ListItems[index].FormatListItem(IsLineBreakRequired, Parent.Configuration).Concat(result)
                : result;
        }

        IEnumerable<Syntax> GetBodyItems()
        {
            var main = Syntax.TokenClass;
            var current = Syntax;
            do
            {
                Tracer.Assert(current != null);
                yield return current.Left;
                current = current.Right;
                if(current == null)
                    yield break;
            }
            while(current.TokenClass == main);

            yield return current;
        }

        IEnumerable<FormatterTokenGroup> GetListItems()
        {
            var main = Syntax.TokenClass;
            var current = Syntax;
            do
            {
                yield return FormatterTokenGroup.Create(current);
                current = current.Right;
            }
            while(current.TokenClass == main);
        }
    }
}