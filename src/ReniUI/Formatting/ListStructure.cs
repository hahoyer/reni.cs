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
        FormatterToken[][] ListItemsValue;

        public ListStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}


        bool HasLineBreak => Syntax.IsLineBreakRequired(Parent.Configuration);

        IStructure[] BodyItems => BodyItemsValue ??
                                  (BodyItemsValue =
                                      GetBodyItems().Select(i => i.CreateListItemStruct(Parent)).ToArray());

        FormatterToken[][] ListItems => ListItemsValue ?? (ListItemsValue = GetListItems().ToArray());

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart)
        {
            for(var i = 0; i <= ListItems.Length; i++)
            {
                if(i > 0)
                {
                    if(Parent.Configuration.SpaceBeforeListItem)
                        yield return SourcePartEditExtension.Space;

                    foreach(var items in ListItems[i - 1])
                        yield return items.ToSourcePartEdit();

                    if(!HasLineBreak && Parent.Configuration.SpaceAfterListItem)
                        yield return SourcePartEditExtension.Space;
                }

                if(HasLineBreak)
                    yield return SourcePartEditExtension.LineBreak;

                foreach(var edit in BodyItems[i].GetSourcePartEdits(targetPart))
                    yield return edit;
            }
        }

        IEnumerable<Syntax> GetBodyItems()
        {
            var main = Syntax.TokenClass;
            var current = Syntax;
            do
            {
                yield return current.Left;
                Tracer.Assert(FormatterToken.Create(Syntax).Count() == 1);
                current = current.Right;
            }
            while(current.TokenClass == main);

            yield return current;
        }

        IEnumerable<FormatterToken[]> GetListItems()
        {
            var main = Syntax.TokenClass;
            var current = Syntax;
            do
            {
                var items = FormatterToken.Create(current).ToArray();
                Tracer.Assert(items.Length == 1);
                yield return items;
                current = current.Right;
            }
            while(current.TokenClass == main);
        }
    }
}