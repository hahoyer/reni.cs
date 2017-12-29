using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class ChainStructure : Structure
    {
        ChainItemStruct[] BodyItemsValue;

        public ChainStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        ChainItemStruct[] BodyItems
            => BodyItemsValue ??
               (BodyItemsValue = GetBodyItems().Select(i => i.CreateChainItemStruct(Parent)).ToArray());

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
            => ConvertToSourcePartEdits(targetPart, exlucdePrefix);

        IEnumerable<ISourcePartEdit> ConvertToSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            var effectiveExcludePrefix = exlucdePrefix;

            foreach(var item in BodyItems)
            {
                if(item.IsTailItem && IsLineBreakRequired)
                    yield return SourcePartEditExtension.LineBreak;

                var edits = ((IStructure) item).GetSourcePartEdits(targetPart, effectiveExcludePrefix);
                foreach(var edit in edits)
                    yield return edit;

                effectiveExcludePrefix = true;
            }
        }

        IEnumerable<Syntax> GetBodyItems()
        {
            var current = Syntax;
            var items = new List<Syntax>();
            while(current.Left != null)
            {
                items.Add(current);
                current = current.Left;
            }

            items.Add(current);
            return items;
        }
    }

    sealed class ChainItemStruct : Structure
    {
        public ChainItemStruct(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        internal bool IsTailItem => Syntax.Left != null;

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            if(IsTailItem)
                yield return SourcePartEditExtension.IndentStart;

            var edits = FormatterTokenGroup
                .Create(Syntax)
                .FormatChainItem(IsLineBreakRequired, Parent.Configuration);

            foreach(var edit in edits)
                yield return edit;

            if(Syntax.Right != null)
                foreach(var edit in Syntax.Right.CreateStruct(Parent).GetSourcePartEdits(targetPart, false))
                    yield return edit;

            if(IsTailItem)
                yield return SourcePartEditExtension.IndentEnd;
        }
    }
}