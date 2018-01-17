using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class ChainStructure : StructureBase
    {
        static IEnumerable<Syntax> GetBodyItems(Syntax syntax)
            => syntax == null ? Enumerable.Empty<Syntax>() : GetBodyItems(syntax.Left).plus(syntax);

        ChainItemStruct[] BodyItemsValue;

        public ChainStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        ChainItemStruct[] BodyItems
            => BodyItemsValue ??
               (BodyItemsValue = GetBodyItems(Syntax).Select(i => i.CreateChainItemStruct(Parent)).ToArray());

        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits
            (SourcePart targetPart, bool exlucdePrefix)
        {
            var effectiveExcludePrefix = exlucdePrefix;

            var edits = new List<ISourcePartEdit>();
            foreach(var item in BodyItems)
            {
                if(item.IsTailItem && IsLineBreakRequired)
                    edits.Add(SourcePartEditExtension.LineBreak);

                edits.AddRange(((IStructure) item).GetSourcePartEdits(effectiveExcludePrefix,false));

                effectiveExcludePrefix = false;
            }

            return edits.SingleToArray();
        }
    }

    sealed class ChainItemStruct : StructureBase
    {
        public ChainItemStruct(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        internal bool IsTailItem => Syntax.Left != null;

        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits
            (SourcePart targetPart, bool exlucdePrefix)
        {
            if(IsTailItem)
                yield return SourcePartEditExtension.ToRight.SingleToArray();

            var tokenGroup = FormatterTokenGroup
                .Create(Syntax);

            var edits = tokenGroup
                .FormatChainItem(exlucdePrefix)
                .SelectMany(i => i)
                .ToArray();

            yield return edits;

            if(Syntax.Right != null)
                yield return Syntax.Right.CreateStruct(Parent).GetSourcePartEdits(false, false);

            if(IsTailItem)
                yield return SourcePartEditExtension.ToLeft.SingleToArray();
        }
    }
}