using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Structure : DumpableObject, IStructure
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

        protected Structure(Syntax syntax, StructFormatter parent)
        {
            Syntax = syntax;
            Parent = parent;
            IsLineBreakRequiredCache = new ValueCache<bool>(() => Syntax.IsLineBreakRequired(Parent.Configuration));
        }

        Syntax IStructure.Syntax => Syntax;

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix)
        {
            var result = GetSourcePartEdits(targetPart, exlucdePrefix).SelectMany(i => i).ToArray();
            AssertValid(result);
            return result;
        }

        [EnableDump]
        protected bool IsLineBreakRequired => IsLineBreakRequiredCache.Value;

        [EnableDump]
        protected string FlatResult => Syntax.FlatFormat(Parent.Configuration);

        protected abstract IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits
            (SourcePart targetPart, bool exlucdePrefix);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Syntax.Token.Characters.Id;
    }
}