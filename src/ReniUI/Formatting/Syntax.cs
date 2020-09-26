using System;
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
    sealed class Syntax : SyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            internal ValueCache<SplitMaster> SplitMaster;
            internal ValueCache<SplitItem> SplitItem;
            internal bool? HasAlreadyLineBreakOrIsTooLong;
        }

        readonly Func<Configuration> GetConfiguration;
        readonly CacheContainer Cache = new CacheContainer();

        internal Syntax(Reni.TokenClasses.Syntax target, Configuration configuration)
            : this(target, null, configuration) {}

        Syntax(Reni.TokenClasses.Syntax target, Syntax parent, Configuration configuration = null)
            : base(target, parent)
        {
            if(parent == null)
                GetConfiguration = () => configuration;
            else
                GetConfiguration = () => Parent.Configuration;
            Tracer.Assert(Target != null);
            Cache.SplitItem = new ValueCache<SplitItem>(GetSplitItem);
            Cache.SplitMaster = new ValueCache<SplitMaster>(GetSplitMaster);
        }

        protected override Syntax Create(Reni.TokenClasses.Syntax target, Syntax parent)
            => new Syntax(target, parent);

        [DisableDump]
        SplitItem SplitItem => Cache.SplitItem.Value;

        [DisableDump]
        SplitMaster SplitMaster => Cache.SplitMaster.Value;

        SplitMaster GetSplitMaster()
        {
            switch(TokenClass)
            {
                case List _:
                    switch(Parent.TokenClass)
                    {
                        case BeginOfText _: 
                        case EndOfText _: return SplitMaster.List[TokenClass];
                        case List _:
                            Tracer.Assert(TokenClass == Parent.TokenClass);
                            return null;
                        default:
                            NotImplementedMethod(nameof(Parent), Parent);
                            return default;
                    }

                case Colon _:
                    switch(Parent.TokenClass)
                    {
                        case Colon _:
                            return null;
                        default:
                            return SplitMaster.Colon[TokenClass];
                    }
                default: return null;
            }
        }

        SplitItem GetSplitItem()
        {
            switch(TokenClass)
            {
                case BeginOfText _:
                case EndOfText _:
                case List _:
                case Colon _: return null;
            }

            switch(Parent.TokenClass)
            {
                case BeginOfText _:
                case EndOfText _: return null;
                case List tokenClass: return SplitItem.List[tokenClass];
                case Colon tokenClass:
                    return IsLeftChild ? SplitItem.ColonLabel[tokenClass] : SplitItem.ColonBody[tokenClass];
            }

            NotImplementedMethod(nameof(Parent), Parent);
            return default;
        }

        [DisableDump]
        int LineBreaks
        {
            get
            {
                switch(TokenClass)
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
                if(Left != null)
                {
                    NotImplementedMethod();
                    return default;
                }

                var item = ItemForLineSplit;
                var master = item?.MasterForLineSplit;

                if(master == null)
                    return 0;

                Tracer.Assert
                (
                    item.SplitItem.TokenClass == master.SplitMaster.TokenClass,
                    () => item.SplitItem.TokenClass + " == " + master.SplitMaster.TokenClass
                );
                if(!master.IsLineSplitRequired)
                    return 0;

                if(master.LeftMost == this)
                    return 0;

                Tracer.Assert(item.LeftMost == this);

                if(item.IsLineSplitRequired)
                    return 2;

                var leftItem = GetLeftNeighborItemForLineSplit(master, item);
                return leftItem?.IsLineSplitRequired == true ? 2 : 1;
            }
        }

        Syntax GetLeftNeighborItemForLineSplit(Syntax parent, Syntax item)
        {
            if(item.Parent.Right == this)
                return item.Parent.Left;

            Tracer.Assert(item.Parent.Left == this);
            Tracer.Assert(item.Parent.LeftMost == this);

            var result = LeftNeighbor?.ItemForLineSplit;
            return result?.MasterForLineSplit == parent ? result : null;
        }

        Syntax ItemForLineSplit => SplitItem == null ? Parent?.ItemForLineSplit : this;
        Syntax MasterForLineSplit => SplitMaster == null ? Parent?.MasterForLineSplit : this;

        [EnableDump]
        new ITokenClass TokenClass => base.TokenClass;

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
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
                        .Indent(IndentDirection);
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

        [DisableDump]
        IEnumerable<ISourcePartEdit> WhiteSpacesEdits
        {
            get
            {
                if(Token.PrecededWith.Any())
                    return T(new WhiteSpaceView(WhiteSpaces, Configuration, IsSeparatorRequired, LineBreaks));
                return T(new EmptyWhiteSpaceView(Token.Characters.Start, IsSeparatorRequired, LineBreaks));
            }
        }

        [DisableDump]
        Configuration Configuration => GetConfiguration();

        [DisableDump]
        IndentDirection IndentDirection => SplitItem?.Indent ?? IndentDirection.NoIndent;

        [DisableDump]
        bool IsLineSplitRequired
            => IsLineSplitRequiredByParent || HasAlreadyLineBreakOrIsTooLong;

        [DisableDump]
        bool IsLineSplitRequiredByParent
            => (TokenClass is List || TokenClass is LeftParenthesis || TokenClass is RightParenthesis) &&
               Parent.IsLineSplitForListRequired;

        [DisableDump]
        bool IsLineSplitForListRequired
            => (TokenClass is List || TokenClass is LeftParenthesis || TokenClass is RightParenthesis) &&
               HasAlreadyLineBreakOrIsTooLong;

        TContainer FlatFormat<TContainer, TValue>(bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
        {
            var tokenString = Token.Characters
                .FlatFormat(WhiteSpaces, areEmptyLinesPossible);

            if(tokenString == null)
                return null;

            tokenString = (IsSeparatorRequired ? " " : "") + tokenString;

            var leftResult = FlatSubFormat<TContainer, TValue>(Left, areEmptyLinesPossible);
            if(leftResult == null)
                return null;

            var rightResult = FlatSubFormat<TContainer, TValue>(Right, areEmptyLinesPossible);
            return rightResult == null ? null : leftResult.Concat(tokenString, rightResult);
        }

        static TContainer FlatSubFormat<TContainer, TValue>(Syntax left, bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
            => left == null ? new TContainer() : left.FlatFormat<TContainer, TValue>(areEmptyLinesPossible);

        [DisableDump]
        internal bool HasAlreadyLineBreakOrIsTooLong
            => Cache.HasAlreadyLineBreakOrIsTooLong ??
               (Cache.HasAlreadyLineBreakOrIsTooLong = GetHasAlreadyLineBreakOrIsTooLong()).Value;

        bool GetHasAlreadyLineBreakOrIsTooLong()
        {
            var basicLineLength = GetFlatLength(Configuration.EmptyLineLimit != 0);
            return basicLineLength == null || basicLineLength > Configuration.MaxLineLength;
        }

        /// <summary>
        ///     Try to format target into one line.
        /// </summary>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The formatted line or null if target contains line breaks.</returns>
        internal string FlatFormat(bool areEmptyLinesPossible)
            => FlatFormat<StringResult, string>(areEmptyLinesPossible)?.Value;

        /// <summary>
        ///     Get the line length of target when formatted as one line.
        /// </summary>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The line length calculated or null if target contains line breaks.</returns>
        int? GetFlatLength(bool areEmptyLinesPossible)
            => FlatFormat<IntegerResult, int>(areEmptyLinesPossible)?.Value;

        [DisableDump]
        IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
        {
            get
            {
                var whiteSpacesEdits = WhiteSpacesEdits.ToArray();
                var result = new List<IEnumerable<ISourcePartEdit>>();

                if(Target.Left != null)
                    result.Add(Left.Edits);

                result.Add(whiteSpacesEdits);

                if(Target.Right != null)
                    result.Add(Right.Edits);
                return result;
            }
        }

        [DisableDump]
        bool IsSeparatorRequired
            => !WhiteSpaces.HasComment() && SeparatorExtension.Get(LeftNeighbor?.TokenClass, TokenClass);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Target.Token.Characters.Id;

        static IEnumerable<TValue> T<TValue>(params TValue[] value) => value;
        static bool True => true;
        static bool False => false;
    }
}