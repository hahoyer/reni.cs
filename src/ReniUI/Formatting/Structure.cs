using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Structure : TreeWithParentExtended<Structure, Syntax>
    {
        public Structure(Syntax target, Configuration configuration)
            : this(target, null, configuration) {}

        Structure(Syntax target, Structure parent, Configuration configuration)
            : base(target, parent)
            => Configuration = parent?.Configuration ?? configuration;

        readonly Configuration Configuration;

        protected override Structure Create(Syntax target, Structure parent)
            => new Structure(target, parent, Configuration);

        ITokenClass TokenClass => Target.TokenClass;

        internal IEnumerable<ISourcePartEdit> Edits
        {
            get
            {
                var trace = False;
                StartMethodDump(trace);
                try
                {
                    var result = EditGroups
                        .SelectMany(i => i)
                        .ToArray()
                        .Indent(Target.IndentDirection);
                    Dump(nameof(result), result);
                    Tracer.ConditionalBreak(trace);
                    return ReturnMethodDump(result, trace);
                }
                finally
                {
                    EndMethodDump();
                }
            }
        }

        IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
        {
            get
            {
                var whiteSpacesEdits = WhiteSpacesEdits.ToArray();
                var result = new List<IEnumerable<ISourcePartEdit>>();

                if(Left != null)
                    result.Add(Left.Edits);

                result.Add(whiteSpacesEdits);

                if(Right != null)
                    result.Add(Right.Edits);
                return result;
            }
        }

        IEnumerable<ISourcePartEdit> WhiteSpacesEdits
        {
            get
            {
                if(Target.Token.PrecededWith.Any())
                    return T
                    (
                        new WhiteSpaceView
                        (
                            Target.WhiteSpaces,
                            Configuration,
                            Target.IsSeparatorRequired,
                            LineBreaks));
                return T
                (
                    new EmptyWhiteSpaceView
                        (Target.Token.Characters.Start, Target.IsSeparatorRequired, LineBreaks));
            }
        }

        int LineBreaks
        {
            get
            {
                switch(Target.TokenClass)
                {
                    case BeginOfText _:
                    case EndOfText _:
                    case List _:
                    case Colon _: return 0;
                }

                return LineBreaksByParent;
            }
        }

        int LineBreaksByParent
        {
            get
            {
                var itemsForLineSplit = ItemsForLineSplit.ToArray();
                var ints = itemsForLineSplit.Select(LineBreaksByParentForItem).ToArray();
                return ints.MaxEx() ?? 0;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<Structure> ItemsForLineSplit
        {
            get
            {
                var result = new List<Structure>();
                var target = this;
                while(true)
                {
                    var item = target.ItemForLineSplit;
                    if(item == null)
                        return result;

                    result.Add(item);
                    target = item.MasterForLineSplit;
                }
            }
        }

        Structure ItemForLineSplit => Target.SplitItem == null ? Parent?.ItemForLineSplit : this;

        Structure MasterForLineSplit
            => Target.SplitItem == null
                ? ItemForLineSplit.MasterForLineSplit
                : GetMasterForLineSplit(Target.SplitItem.TokenClass);

        Structure GetMasterForLineSplit(ITokenClass tokenClass)
            => Target.SplitMaster?.TokenClass == tokenClass
                ? this
                : Parent?.GetMasterForLineSplit(tokenClass);


        int LineBreaksByParentForItem(Structure item)
        {
            var master = item.MasterForLineSplit;

            Tracer.Assert(master != null);

            Tracer.Assert
            (
                item.Target.SplitItem.TokenClass == master.Target.SplitMaster.TokenClass,
                () => item.Target.SplitItem.TokenClass + " == " + master.Target.SplitMaster.TokenClass
            );

            if(!master.IsLineSplitRequired)
                return 0;

            if(item.Target.LeftMost != Target)
                return 0;

            if(master.Target.LeftMost == Target)
                return 0;

            if(item.IsLineSplitRequired)
                return 2;

            var leftItem = GetLeftNeighborItemForLineSplit(master, item);
            return leftItem.IsLineSplitRequired ? 2 : 1;
        }

        Structure GetLeftNeighborItemForLineSplit(Structure master, Structure item)
        {
            if(item.Parent.Right == item)
                return item.Parent.Left;

            Tracer.Assert(item.Parent.Left == item, () => item.Parent.Dump());
            Tracer.Assert(item.Parent.Target.LeftMost == Target);

            var result = LeftNeighbor
                .Chain(target => target.LeftNeighbor)
                .FirstOrDefault(target => target.ItemForLineSplit != null)
                ?.ItemForLineSplit;
            return result?.MasterForLineSplit == master ? result : null;
        }

        bool IsLineSplitRequired => IsLineSplitRequiredByParent || HasAlreadyLineBreakOrIsTooLong;

        bool IsLineSplitRequiredByParent =>
            (TokenClass is List || TokenClass is LeftParenthesis || TokenClass is RightParenthesis) &&
            Parent.IsLineSplitForListRequired;

        bool IsLineSplitForListRequired =>
            (TokenClass is List || TokenClass is LeftParenthesis || TokenClass is RightParenthesis) &&
            HasAlreadyLineBreakOrIsTooLong;

        bool HasAlreadyLineBreakOrIsTooLong
            => this.CachedValue(GetHasAlreadyLineBreakOrIsTooLong);

        bool GetHasAlreadyLineBreakOrIsTooLong()
        {
            var basicLineLength = Target.GetFlatLength(Configuration.EmptyLineLimit != 0);
            return basicLineLength == null || basicLineLength > Configuration.MaxLineLength;
        }

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Target.Token.Characters.Id;

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
        static bool True => true;
        static bool False => false;
    }
}