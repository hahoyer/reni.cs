using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    class ListStructure : DumpableObject, IStructure
    {
        readonly StructFormatter Parent;

        [EnableDump]
        readonly Syntax Syntax;

        IStructure[] BodyItemsValue;
        FormatterToken[][] ListItemsValue;

        public ListStructure(Syntax syntax, StructFormatter parent)
        {
            Syntax = syntax;
            Parent = parent;
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(SourcePart targetPart)
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

        bool IStructure.LineBreakScan(ref int? lineLength) => LineBreakScan(ref lineLength);

        bool HasLineBreak
        {
            get
            {
                var lineLength = Parent.Configuration.MaxLineLength;
                return LineBreakScan(ref lineLength);
            }
        }

        IStructure[] BodyItems => BodyItemsValue ??
                                  (BodyItemsValue =
                                      GetBodyItems().Select(i => i.CreateListItemStruct(Parent)).ToArray());

        FormatterToken[][] ListItems => ListItemsValue ?? (ListItemsValue = GetListItems().ToArray());

        bool LineBreakScan(ref int? lineLength)
        {
            for(var i = 0; i <= ListItems.Length; i++)
            {
                if(i > 0 && ListItems[i - 1].LineBreakScan(ref lineLength))
                    return true;

                if(BodyItems[i].LineBreakScan(ref lineLength))
                    return true;
            }
            return false;
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