using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class BinaryTreeSyntax : BinaryTreeSyntaxWithParent<BinaryTreeSyntax>
    {
        class CacheContainer
        {
            internal ValueCache<SplitItem> SplitItem;
            internal ValueCache<SplitMaster> SplitMaster;
        }

        readonly CacheContainer Cache = new CacheContainer();

        internal BinaryTreeSyntax(BinaryTree target)
            : this(target, null) { }

        BinaryTreeSyntax(BinaryTree target, BinaryTreeSyntax parent)
            : base(target, parent)
        {
            Tracer.Assert(Target != null);
            Cache.SplitItem = new ValueCache<SplitItem>(GetSplitItem);
            Cache.SplitMaster = new ValueCache<SplitMaster>(GetSplitMaster);
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal SplitItem SplitItem => Cache.SplitItem.Value;

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal SplitMaster SplitMaster => Cache.SplitMaster.Value;

        [EnableDump]
        new ITokenClass TokenClass => base.TokenClass;

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal IndentDirection IndentDirection => SplitItem?.Indent ?? IndentDirection.NoIndent;

        [DisableDump]
        internal bool IsSeparatorRequired
            => !WhiteSpaces.HasComment() && SeparatorExtension.Get(LeftNeighbor?.TokenClass, TokenClass);

        static TContainer FlatSubFormat<TContainer, TValue>(BinaryTreeSyntax left, bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
            => left == null? new TContainer() : left.FlatFormat<TContainer, TValue>(areEmptyLinesPossible);

        protected override BinaryTreeSyntax Create(BinaryTree target, BinaryTreeSyntax parent)
            => new BinaryTreeSyntax(target, parent);

        SplitMaster GetSplitMaster()
        {
            switch(TokenClass)
            {
                case List tokenClass:
                    switch(Parent.TokenClass)
                    {
                        case BeginOfText _:
                        case EndOfText _:
                            return SplitMaster.List[tokenClass];
                        case List _:
                            Tracer.Assert(tokenClass == Parent.TokenClass);
                            return null;
                        default:
                            NotImplementedMethod(nameof(Parent), Parent);
                            return default;
                    }

                case Colon tokenClass:
                    switch(Parent.TokenClass)
                    {
                        case Colon _:
                            return null;
                        default:
                            return SplitMaster.Colon[tokenClass];
                    }

                case RightParenthesis tokenClass:
                    Tracer.Assert
                    (
                        Left?.TokenClass is RightParenthesis && Left.TokenClass.IsBelongingTo(tokenClass)
                    );
                    return SplitMaster.Parenthesis[Left.TokenClass];
                default:
                    return null;
            }
        }

        SplitItem GetSplitItem()
        {
            switch(TokenClass)
            {
                case BeginOfText _:
                case EndOfText _:
                    return null;
                case LeftParenthesis tokenClass:
                    Tracer.Assert(Parent.TokenClass.IsBelongingTo(tokenClass));
                    return SplitItem.LeftParenthesis[tokenClass];
            }

            switch(Parent.TokenClass)
            {
                case BeginOfText _:
                case EndOfText _:
                    return null;
                case Colon tokenClass:
                    Tracer.Assert(!(TokenClass is Colon));
                    return IsLeftChild? SplitItem.ColonLabel[tokenClass] : SplitItem.ColonBody[tokenClass];
                case LeftParenthesis tokenClass:
                    return SplitItem.List[tokenClass];
                case List tokenClass:
                    var master = Parent
                        .Chain(target => target.Parent)
                        .SkipWhile(target => target.TokenClass == tokenClass)
                        .First();
                    var masterTokenClass = master.TokenClass is LeftParenthesis? master.TokenClass : tokenClass;
                    return SplitItem.List[masterTokenClass];
                default:
                    NotImplementedMethod(nameof(Parent), Parent);
                    return default;
            }
        }

        TContainer FlatFormat<TContainer, TValue>(bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
        {
            var tokenString = Token.Characters
                .FlatFormat(Left == null? null : WhiteSpaces, areEmptyLinesPossible);

            if(tokenString == null)
                return null;

            tokenString = (IsSeparatorRequired? " " : "") + tokenString;

            var leftResult = FlatSubFormat<TContainer, TValue>(Left, areEmptyLinesPossible);
            if(leftResult == null)
                return null;

            var rightResult = FlatSubFormat<TContainer, TValue>(Right, areEmptyLinesPossible);
            return rightResult == null? null : leftResult.Concat(tokenString, rightResult);
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
        internal int? GetFlatLength(bool areEmptyLinesPossible)
            => FlatFormat<IntegerResult, int>(areEmptyLinesPossible)?.Value;

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Target.Token.Characters.Id;
    }
}