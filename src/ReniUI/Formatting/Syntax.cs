using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;

namespace ReniUI.Formatting
{
    sealed class Syntax : SyntaxView<Syntax>
    {
        class CacheContainer
        {
            internal ValueCache<SplitItem> SplitItem;
            internal ValueCache<SplitMaster> SplitMaster;
        }

        readonly CacheContainer Cache = new CacheContainer();
        readonly Configuration Configuration;

        bool IsIndentRequired;
        bool ForceLineSplit;

        internal Syntax(Reni.SyntaxTree.Syntax flatItem, Configuration configuration)
            : this(flatItem, new PositionDictionary<Syntax>(), 0, null)
            => Configuration = configuration;

        Syntax(Reni.SyntaxTree.Syntax flatItem, PositionDictionary<Syntax> context, int index, Syntax parent)
            : base(flatItem, parent, context, index)
        {
            if(parent != null)
                Configuration = parent.Configuration;
        }

        bool IsLineSplit => ForceLineSplit || GetHasAlreadyLineBreakOrIsTooLong(this);

        bool GetHasAlreadyLineBreakOrIsTooLong(Syntax target)
        {
            var basicLineLength = target.GetFlatLength(Configuration.EmptyLineLimit != 0);
            return basicLineLength == null || basicLineLength > Configuration.MaxLineLength;
        }

        [EnableDump]
        new Reni.SyntaxTree.Syntax FlatItem => base.FlatItem;

        [EnableDump]
        [EnableDumpExcept(null)]
        string ParentToken => Parent?.Anchors.DumpSource();

        [EnableDump(Order = 10)]
        string[] Children => FlatItem
            .Children
            .Select(node => node?.Anchor.SourceParts.DumpSource())
            .ToArray();

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal IEnumerable<ISourcePartEdit> Edits
        {
            get
            {
                var trace = ObjectId == -240;
                StartMethodDump(trace);
                try
                {
                    var result = EditGroups
                        .SelectMany(i => i)
                        .ToArray()
                        .Indent(IndentDirection);
                    Dump(nameof(result), result);
                    //Tracer.Assert(CheckMultilineExpectations(result), Anchor.Dump);

                    Tracer.ConditionalBreak(trace);
                    return ReturnMethodDump(result, trace);
                }
                finally
                {
                    EndMethodDump();
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IndentDirection IndentDirection => IsIndentRequired? IndentDirection.ToRight : IndentDirection.NoIndent;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
        {
            get
            {
                if(FlatItem is CompoundSyntax compound)
                    return EditCompound;
                NotImplementedMethod();
                return default;
            }
        }

        IEnumerable<IEnumerable<ISourcePartEdit>> EditCompound
        {
            get
            {
                FlatFormat()

                NotImplementedMethod();
                return default;
            }
        }

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index)
            => new Syntax(flatItem, Context, index, this);
    }
}