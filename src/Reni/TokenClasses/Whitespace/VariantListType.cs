using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    abstract class VariantListType : DumpableObject, IItemsType, IItemType
    {
        IEnumerable<WhitespaceItem> IItemsType.GetItems(SourcePart sourcePart, IParent parent)
        {
            var sourcePosition = sourcePart.Start.Clone;
            while(sourcePosition < sourcePart.End)
            {
                var valueTuples = VariantPrototypes
                    .Select(p => (p.Type, Length: sourcePosition.Span(sourcePart.End).Match(p.Match)))
                    .ToArray();
                var result = valueTuples
                    .FirstOrDefault(p => p.Length != null);

                result.AssertIsNotNull();
                result.Length.AssertIsNotNull();
                Tracer.ConditionalBreak(result.Length == 0);

                var resultingSourcePart = sourcePosition.Span(T(result.Length.Value, sourcePart.EndPosition).Min());
                (resultingSourcePart.End <= sourcePart.End).Assert();
                yield return new(result.Type, resultingSourcePart, parent);
                sourcePosition += result.Length.Value;
            }
        }

        bool IItemType.IsSeparatorRequired => IsSeparatorRequired;

        [DisableDump]
        protected abstract ItemPrototype[] VariantPrototypes { get; }

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        [DisableDump]
        protected virtual bool IsSeparatorRequired => false;
    }
}