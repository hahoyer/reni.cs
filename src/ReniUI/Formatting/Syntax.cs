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
    sealed class Syntax : SyntaxView<Syntax>
    {
        class CacheContainer
        {
            internal ValueCache<SplitItem> SplitItem;
            internal ValueCache<SplitMaster> SplitMaster;
        }

        readonly CacheContainer Cache = new CacheContainer();

        internal Syntax(Reni.SyntaxTree.Syntax flatItem)
            : this(flatItem, new PositionDictionary<Syntax>(), 0, null) { }

        Syntax(Reni.SyntaxTree.Syntax flatItem, PositionDictionary<Syntax> context, int index, Syntax parent)
            : base(flatItem, parent, context, index) { }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal SplitItem SplitItem => Cache.SplitItem.Value;

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal SplitMaster SplitMaster => Cache.SplitMaster.Value;

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal IndentDirection IndentDirection => SplitItem?.Indent ?? IndentDirection.NoIndent;

        bool GetIsSeparatorRequired(BinaryTree current)
            => !current.Token.PrecededWith.HasComment() &&
               SeparatorExtension.Get(GetLeftNeighbor(current)?.TokenClass, current.TokenClass);

        internal BinaryTree GetLeftNeighbor(BinaryTree current)
        {
            NotImplementedMethod(current);
            return default;
        }

        protected override Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index)
            => new Syntax(flatItem, Context, index, this);

        SplitMaster GetSplitMaster()
        {
            NotImplementedMethod();
            return default;
        }

        SplitItem GetSplitItem()
        {
            NotImplementedMethod();
            return default;
        }

        TContainer FlatSubFormat<TContainer, TValue>(BinaryTree left, bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
            => left == null? new TContainer() : FlatFormat<TContainer, TValue>(left, areEmptyLinesPossible);

        TContainer FlatFormat<TContainer, TValue>(BinaryTree target, bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
        {
            var tokenString = target.Token.Characters
                .FlatFormat(target.Left == null? null : target.Token.PrecededWith, areEmptyLinesPossible);

            if(tokenString == null)
                return null;

            tokenString = (GetIsSeparatorRequired(target)? " " : "") + tokenString;

            var leftResult = FlatSubFormat<TContainer, TValue>(target.Left, areEmptyLinesPossible);
            if(leftResult == null)
                return null;

            var rightResult = FlatSubFormat<TContainer, TValue>(target.Right, areEmptyLinesPossible);
            return rightResult == null? null : leftResult.Concat(tokenString, rightResult);
        }

        IEnumerable<TResult> FlatFormat<TContainer, TResult>(bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TResult>, new()
        {
            var results = FlatItem
                .Anchor
                .Items
                .Select(item => FlatFormat<TContainer, TResult>(item, areEmptyLinesPossible));

            return results.Any(item => item == null)
                ? null 
                : results.Select(item=>item.Value);
        }

        /// <summary>
        ///     Try to format target into one line.
        /// </summary>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The formatted line or null if target contains line breaks.</returns>
        internal string FlatFormat(bool areEmptyLinesPossible) 
            => FlatFormat<StringResult, string>(areEmptyLinesPossible)
                ?.Stringify("");

        /// <summary>
        ///     Get the line length of target when formatted as one line.
        /// </summary>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The line length calculated or null if target contains line breaks.</returns>
        internal int? GetFlatLength(bool areEmptyLinesPossible) 
            => FlatFormat<IntegerResult, int>(areEmptyLinesPossible)
                ?.Sum();
    }
}