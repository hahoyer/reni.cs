using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class StructureBase : DumpableObject, IStructure
    {
        static void AssertValid(IEnumerable<ISourcePartEdit> result)
        {
            var currentPosition = 0;
            var currentIndex = 0;

            foreach(var edit in result)
            {
                if(edit is SourcePartEdit s)
                {
                    var part = ((ISourcePartProxy) s.Source).All;
                    Tracer.Assert(part.Position >= currentPosition);
                    currentPosition = part.EndPosition;
                }

                currentIndex++;
            }
        }

        readonly ValueCache<bool> IsLineBreakRequiredCache;
        protected readonly StructFormatter Parent;
        protected readonly Syntax Syntax;

        protected StructureBase(Syntax syntax, StructFormatter parent)
        {
            Syntax = syntax;
            Parent = parent;
            IsLineBreakRequiredCache = new ValueCache<bool>(() => Syntax.IsLineBreakRequired(Parent.Configuration));
            Tracer.Assert(Syntax != null);
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit>
            IStructure.GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix, bool includeSuffix)
        {
            var lr = IsLineBreakRequired;
            var result = new List<ISourcePartEdit>();

            if(Syntax.Left != null)
                result.AddRange(GetLeftSiteEdits(targetPart, exlucdePrefix));

            result.AddRange(GetMainAndSiteEdits(targetPart, includeSuffix));
            AssertValid(result);
            return result;
        }

        [EnableDump]
        protected bool IsLineBreakRequired => IsLineBreakRequiredCache.Value;

        [EnableDump]
        protected string FlatResult => Syntax.FlatFormat(Parent.Configuration);

        protected virtual IEnumerable<ISourcePartEdit> GetMainAndSiteEdits(SourcePart targetPart, bool includeSuffix)
        {
            var main = FormatterTokenGroup.Create(Syntax);

            IEnumerable<ISourcePartEdit> result = main.Main;

            if(Syntax.Right != null || includeSuffix)
                result = result.Concat(main.Suffix);

            if(Syntax.Right != null)
                result = result.Concat(GetRightSiteEdits(targetPart, includeSuffix));

            return result;
        }

        protected virtual IEnumerable<ISourcePartEdit> GetLeftSiteEdits(SourcePart targetPart, bool exlucdePrefix)
            => Syntax.Left.CreateStruct(Parent).GetSourcePartEdits(targetPart, exlucdePrefix, false);

        protected virtual IEnumerable<ISourcePartEdit> GetRightSiteEdits(SourcePart targetPart, bool includeSuffix)
            => Syntax.Right.CreateStruct(Parent).GetSourcePartEdits(targetPart, true, includeSuffix);

        protected virtual IEnumerable<IEnumerable<ISourcePartEdit>>
            GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix) => null;

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}