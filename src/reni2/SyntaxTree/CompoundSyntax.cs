using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    sealed class CompoundSyntax : ValueSyntax
    {
        static readonly bool NoFileDump = true;
        static bool IsInContainerDump;

        static readonly string RunId = Extension.GetFormattedNow() + "\n";
        static bool IsInsideFileDump;
        static int NextObjectId;

        [EnableDump(Order = 2)]
        internal readonly CleanupSyntax CleanupSection;

        [EnableDump(Order = 1)]
        internal readonly IStatementSyntax[] Statements;

        CompoundSyntax
        (
            IStatementSyntax[] statements, CleanupSyntax cleanupSection, BinaryTree anchor
            , FrameItemContainer frameItems
        )
            : base(NextObjectId++, anchor, frameItems)
        {
            Statements = statements;
            CleanupSection = cleanupSection;

            AssertValid();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

        [DisableDump]
        public IEnumerable<FunctionSyntax> ConverterFunctions
            => Statements
                .Where(data => data.Declarer?.IsConverterSyntax ?? false)
                .Select(data => (FunctionSyntax)data.Value);

        [DisableDump]
        internal ValueSyntax[] PureStatements => Statements.Select(s => s.Value).ToArray();

        [EnableDump(Order = 100)]
        internal IDictionary<string, int> NameIndex
            => Statements
                .Select((statement, index) => (Key: statement.Declarer?.Name?.Value, Value: index))
                .Where(pair => pair.Key != null)
                .ToDictionary(item => item.Key, item => item.Value);

        [EnableDump(Order = 100)]
        internal int[] MutableDeclarations => IndexList(item => item.IsMutableSyntax).ToArray();

        [EnableDump(Order = 100)]
        internal int[] Converters => IndexList(item => item.IsConverterSyntax).ToArray();

        [EnableDump(Order = 100)]
        internal int[] MixInDeclarations => IndexList(item => item.IsMixInSyntax).ToArray();

        [DisableDump]
        internal int EndPosition => PureStatements.Length;


        [DisableDump]
        internal override bool? IsHollow => PureStatements.All(syntax => syntax.IsHollow == true);

        [DisableDump]
        internal Size IndexSize => Size.AutoSize(PureStatements.Length);

        [DisableDump]
        internal string[] AllNames => Statements
            .Select(s => s.Declarer?.Name?.Value)
            .Where(name => name != null)
            .ToArray();

        [DisableDump]
        internal int[] ConverterStatementPositions
            => Statements
                .SelectMany((s, i) => s.Declarer.IsConverterSyntax? new[] {i} : new int[0])
                .ToArray();


        [DisableDump]
        protected override int LeftDirectChildCountKernel => DirectChildCountKernel;

        [DisableDump]
        protected override int DirectChildCountKernel => Statements.Length + 1;

        public static CompoundSyntax Create
        (
            IStatementSyntax[] statements,
            BinaryTree anchor,
            CleanupSyntax cleanupSection = null,
            FrameItemContainer frameItems = null
        )
            => new CompoundSyntax
            (
                statements,
                cleanupSection,
                anchor,
                frameItems
            );

        public string GetCompoundIdentificationDump() => "." + ObjectId + "i";

        public override string DumpData()
        {
            var isInsideFileDump = IsInsideFileDump;
            IsInsideFileDump = true;
            var result = NoFileDump || isInsideFileDump? DumpDataToString() : DumpDataToFile();
            IsInsideFileDump = isInsideFileDump;
            return result;
        }

        protected override string GetNodeDump()
            => GetType().PrettyName() + "(" + GetCompoundIdentificationDump() + ")";

        protected override Syntax GetDirectChildKernel(int index)
        {
            if(index >= 0 && index < Statements.Length)
                return (Syntax)Statements[index];
            return index == Statements.Length? CleanupSection : null;
        }

        internal bool IsMutable(int position) => Statements[position].Declarer.IsMutableSyntax;

        internal int? Find(string name, bool publicOnly)
        {
            if(name == null)
                return null;

            return Statements
                .Select((data, index) => data.Declarer?.IsDefining(name, publicOnly) ?? false? index : (int?)null)
                .FirstOrDefault(data => data != null);
        }

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var compound = context.Compound(this);
            return compound.HasIssue? Feature.Extension.Result(compound.Issues, category) : compound.Result(category);
        }

        internal override ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            var statements = Statements.Select(s => s.Value.Visit(visitor)).ToArray();
            var cleanupSection = CleanupSection?.Value.Visit(visitor);

            if(statements.All(s => s == null) && cleanupSection == null)
                return null;

            var newStatements = statements
                .Select((s, i) => s ?? Statements[i])
                .ToArray();

            var newCleanupSection
                = cleanupSection == null
                    ? CleanupSection
                    : new CleanupSyntax(CleanupSection.Anchor, cleanupSection);

            return Create(newStatements, Anchor, newCleanupSection, FrameItems);
        }

        protected override Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree listTarget = null) => this;

        internal Result Cleanup(ContextBase context, Category category)
        {
            if(CleanupSection != null && (category.HasCode || category.HasClosures))
                return context
                           .Result(category.WithType, CleanupSection.Value)
                           .Conversion(context.RootContext.VoidType)
                           .LocalBlock(category) &
                       category;

            return context.RootContext.VoidType.Result(category);
        }

        IEnumerable<int> IndexList(Func<DeclarerSyntax, bool> selector)
        {
            for(var index = 0; index < Statements.Length; index++)
            {
                var declarer = Statements[index].Declarer;
                if(declarer != null && selector(declarer))
                    yield return index;
            }
        }

        string DumpDataToFile()
        {
            var dumpFile = ("compound." + ObjectId).ToSmbFile();
            var oldResult = dumpFile.String;
            var newResult = (RunId + DumpDataToString()).Replace("\n", "\r\n");
            if(oldResult == null || !oldResult.StartsWith(RunId))
            {
                oldResult = newResult;
                dumpFile.String = oldResult;
            }
            else
                (oldResult == newResult).Assert();

            return Tracer.FilePosition(dumpFile.FullName, 1, 0, FilePositionTag.Debug) + "see there\n";
        }

        string DumpDataToString()
        {
            var isInDump = IsInContainerDump;
            IsInContainerDump = true;
            var result = base.DumpData();
            IsInContainerDump = isInDump;
            return result;
        }
    }

    interface IStatementSyntax
    {
        ValueSyntax Value { get; }
        DeclarerSyntax Declarer { get; }
        SourcePart SourcePart { get; }
        ValueSyntax ToValueSyntax(BinaryTree anchor);
        IStatementSyntax With(FrameItemContainer frameItems);
    }
}