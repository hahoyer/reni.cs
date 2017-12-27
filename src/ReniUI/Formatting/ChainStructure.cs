using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;


namespace ReniUI.Formatting
{
    sealed class ChainStructure : Structure
    {
        IStructure[] BodyItemsValue;

        public ChainStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure[] BodyItems
            => BodyItemsValue
               ?? (BodyItemsValue = GetBodyItems().Select(i => i.CreateListItemStruct(Parent)).ToArray());

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix) 
            => ConvertToSourcePartEdits(targetPart, exlucdePrefix).SelectMany(i=>i);

        IEnumerable<IEnumerable<ISourcePartEdit>> ConvertToSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            var bodyItems = BodyItems;

            yield return bodyItems.First().GetSourcePartEdits(targetPart, exlucdePrefix);

            yield return new[]{SourcePartEditExtension.IndentStart};
            
            foreach(var item in bodyItems.Skip(1))
            {
                if(IsLineBreakRequired)
                    yield return new[]{SourcePartEditExtension.LineBreak};

                yield return item.GetSourcePartEdits(targetPart, exlucdePrefix);
            }

            yield return new[]{SourcePartEditExtension.IndentEnd};
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
    }
}