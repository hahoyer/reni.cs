using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Type;
using Base=Reni.Context.Base;

namespace Reni
{
    /// <summary>
    /// Then-else structure
    /// </summary>
    public sealed class ThenElse : Syntax.Base
    {
        private readonly Syntax.Base _condSyntax;
        private readonly Syntax.Base _thenSyntax;
        private readonly Syntax.Base _elseSyntax;

        private readonly Token _thenToken;
        private readonly Token _elseToken;

        public ThenElse(Syntax.Base condSyntax, Token thenToken, Syntax.Base thenSyntax)
        {
            _condSyntax = condSyntax;
            _thenToken = thenToken;
            _thenSyntax = thenSyntax;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ThenElse"/> class.
        /// </summary>
        /// <param name="condSyntax">The cond syntax.</param>
        /// <param name="thenToken">The then token.</param>
        /// <param name="thenSyntax">The then syntax.</param>
        /// <param name="elseToken">The else token.</param>
        /// <param name="elseSyntax">The else syntax.</param>
        /// created 08.01.2007 23:56
        public ThenElse(Syntax.Base condSyntax, Token thenToken, Syntax.Base thenSyntax, Token elseToken, Syntax.Base elseSyntax)
        {
            _condSyntax = condSyntax;
            _thenToken = thenToken;
            _thenSyntax = thenSyntax;
            _elseToken = elseToken;
            _elseSyntax = elseSyntax;
        }

        internal Syntax.Base CondSyntax { get { return _condSyntax; } }
        internal Syntax.Base ThenSyntax { get { return _thenSyntax; } }
        internal Syntax.Base ElseSyntax { get { return _elseSyntax; } }

        internal Token ThenToken { get { return _thenToken; } }

        /// <summary>
        /// Visits the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 08.01.2007 23:49
        override public Result VirtVisit(Context.Base context, Category category)
        {
            Result condResult = _condSyntax.Visit(context, category | Category.Type);
            condResult = condResult.Type.Conversion(category, Bit.CreateBit).UseWithArg(condResult);

            Result thenResult = _thenSyntax.Visit(context, category | Category.Type);
            Result elseResult;
            if (_elseSyntax != null)
                elseResult = _elseSyntax.Visit(context, category | Category.Type);
            else 
                elseResult = Void.CreateVoidResult(category | Category.Type);

            if (thenResult.Type.IsPending)
                return elseResult.Type.ThenElseWithPending(category, condResult.Refs, elseResult.Refs);
            if (elseResult.Type.IsPending)
                return thenResult.Type.ThenElseWithPending(category, condResult.Refs, thenResult.Refs);

            Type.Base commonType = thenResult.Type.CommonType(elseResult.Type);
            
            thenResult = thenResult.Type.Conversion(category, commonType).UseWithArg(thenResult);
            elseResult = elseResult.Type.Conversion(category, commonType).UseWithArg(elseResult);

            return commonType.CreateResult
                (
                category,
                delegate { return condResult.Code.CreateThenElse(thenResult.Code, elseResult.Code); },
                delegate { return condResult.Refs.Pair(thenResult.Refs).Pair(elseResult.Refs); }
                );
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            string result = _condSyntax.DumpShort() + "then" + _thenSyntax.DumpShort();
            if (_elseSyntax != null)
                result += "else" + _elseSyntax.DumpShort();
            return result;
        }
    }
}