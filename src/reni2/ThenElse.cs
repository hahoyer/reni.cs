using Reni.Context;
using Reni.Parser;
using Reni.Syntax;
using Reni.Type;

namespace Reni
{
    internal sealed class ThenElse : CompileSyntax
    {
        private readonly ICompileSyntax _condSyntax;
        private readonly ICompileSyntax _thenSyntax;
        private readonly Token _elseToken;
        private readonly ICompileSyntax _elseSyntax;

        public ThenElse(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax) : base(thenToken)
        {
            _condSyntax = condSyntax;
            _thenSyntax = thenSyntax;
        }

        public ThenElse(ICompileSyntax condSyntax, Token thenToken, ICompileSyntax thenSyntax, Token elseToken, ICompileSyntax elseSyntax)
            : base(thenToken)
        {
            _condSyntax = condSyntax;
            _thenSyntax = thenSyntax;
            _elseToken = elseToken;
            _elseSyntax = elseSyntax;
        }

        internal ICompileSyntax CondSyntax { get { return _condSyntax; } }
        internal ICompileSyntax ThenSyntax { get { return _thenSyntax; } }
        internal ICompileSyntax ElseSyntax { get { return _elseSyntax; } }

        internal Token ElseToken { get { return _elseToken; } }

        internal protected override Result Result(ContextBase context, Category category)
        {
            var condResult = _condSyntax.Result(context, category | Category.Type);
            condResult = condResult.Type.Conversion(category, TypeBase.CreateBit).UseWithArg(condResult);

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

        private Result CreateElseResult(ContextBase context, Category category)
        {
            if(_elseSyntax == null)
                return TypeBase.CreateVoidResult(category | Category.Type);
            return _elseSyntax.Result(context, category | Category.Type);
        }

        internal protected override string DumpShort()
        {
            var result = _condSyntax.DumpShort() + "then" + _thenSyntax.DumpShort();
            if(_elseSyntax != null)
                result += "else" + _elseSyntax.DumpShort();
            return result;
        }
    }
}