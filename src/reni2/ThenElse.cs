using Reni.Parser;
using Reni.Syntax;

namespace Reni
{
#if false
    /// <summary>
    /// Then-else structure
    /// </summary>
    internal sealed class ThenElse : SyntaxBase
    {
        private readonly SyntaxBase _condSyntax;
        private readonly SyntaxBase _elseSyntax;

        private readonly Token _elseToken;
        private readonly SyntaxBase _thenSyntax;
        private readonly Token _thenToken;

        public ThenElse(SyntaxBase condSyntax, Token thenToken, SyntaxBase thenSyntax)
        {
            _condSyntax = condSyntax;
            _thenToken = thenToken;
            _thenSyntax = thenSyntax;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThenElse"/> class.
        /// </summary>
        /// <param name="condSyntax">The cond syntax.</param>
        /// <param name="thenToken">The then token.</param>
        /// <param name="thenSyntax">The then syntax.</param>
        /// <param name="elseToken">The else token.</param>
        /// <param name="elseSyntax">The else syntax.</param>
        /// created 08.01.2007 23:56
        public ThenElse(SyntaxBase condSyntax, Token thenToken, SyntaxBase thenSyntax, Token elseToken, SyntaxBase elseSyntax)
        {
            _condSyntax = condSyntax;
            _thenToken = thenToken;
            _thenSyntax = thenSyntax;
            _elseToken = elseToken;
            _elseSyntax = elseSyntax;
        }

        internal SyntaxBase CondSyntax { get { return _condSyntax; } }
        internal SyntaxBase ThenSyntax { get { return _thenSyntax; } }
        internal SyntaxBase ElseSyntax { get { return _elseSyntax; } }

        internal Token ThenToken { get { return _thenToken; } }
        internal Token ElseToken { get { return _elseToken; } }

        /// <summary>
        /// Visits the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 08.01.2007 23:49
        public Result VirtVisit(Context.ContextBase context, Category category)
        {
            var condResult = _condSyntax.Result(context, category | Category.Type);
            condResult = condResult.Type.Conversion(category, Type.TypeBase.CreateBit).UseWithArg(condResult);

            var thenResult = _thenSyntax.Result(context, category | Category.Type);
            var elseResult = CreateElseResult(context, category);

            if(thenResult.Type.IsPending)
                return elseResult.Type.ThenElseWithPending(category, condResult.Refs, elseResult.Refs);
            if(elseResult.Type.IsPending)
                return thenResult.Type.ThenElseWithPending(category, condResult.Refs, thenResult.Refs);

            var commonType = thenResult.Type.CommonType(elseResult.Type);

            thenResult = thenResult.Type.Conversion(category, commonType).UseWithArg(thenResult);
            elseResult = elseResult.Type.Conversion(category, commonType).UseWithArg(elseResult);

            return commonType.CreateResult
                (
                category,
                () => condResult.Code.CreateThenElse(thenResult.Code, elseResult.Code),
                () => condResult.Refs.Pair(thenResult.Refs).Pair(elseResult.Refs)
                );
        }

        private Result CreateElseResult(Context.ContextBase context, Category category)
        {
            if(_elseSyntax == null)
                return Type.TypeBase.CreateVoidResult(category | Category.Type);
            return _elseSyntax.Result(context, category | Category.Type);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            var result = _condSyntax.DumpShort() + "then" + _thenSyntax.DumpShort();
            if(_elseSyntax != null)
                result += "else" + _elseSyntax.DumpShort();
            return result;
        }

    }
#endif
}