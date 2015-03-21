using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Numeric;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class MainTokenFactory : TokenFactory
    {
        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.Left
                    (
                        AtToken.TokenId,
                        "_N_E_X_T_",
                        ToNumberOfBase.TokenId
                    );

                x += PrioTable.Left(ConcatArrays.TokenId, ConcatArrays.MutableId);

                x += PrioTable.Left(Star.TokenId, Slash.TokenId, "\\");
                x += PrioTable.Left(Plus.TokenId, Minus.TokenId);

                x += PrioTable.Left
                    (
                        CompareOperation.TokenId(),
                        CompareOperation.TokenId(canBeEqual: true),
                        CompareOperation.TokenId(false),
                        CompareOperation.TokenId(false, true)
                    );
                x += PrioTable.Left(EqualityOperation.TokenId(false), EqualityOperation.TokenId());

                x += PrioTable.Right(ReassignToken.TokenId);

                x = x.ThenElseLevel(ThenToken.TokenId, ElseToken.TokenId);
                x += PrioTable.Right(Exclamation.TokenId);
                x += PrioTable.Left
                    (
                        Function.TokenId(),
                        Function.TokenId(true),
                        Function.TokenId(isMetaFunction: true));
                x += PrioTable.Right(Colon.TokenId);
                x += PrioTable.Right(List.TokenId(0));
                x += PrioTable.Right(List.TokenId(1));
                x += PrioTable.Right(List.TokenId(2));
                x = x.ParenthesisLevelLeft
                    (
                        new[]
                        {
                            LeftParenthesis.TokenId(1),
                            LeftParenthesis.TokenId(2),
                            LeftParenthesis.TokenId(3)
                        },
                        new[]
                        {
                            RightParenthesis.TokenId(1),
                            RightParenthesis.TokenId(2),
                            RightParenthesis.TokenId(3)
                        }
                    );
                //x.Correct("(", PrioTable.Any, '-');
                //x.Correct("[", PrioTable.Any, '-');
                //x.Correct("{", PrioTable.Any, '-');

                x += PrioTable.Right(PrioTable.Error);

                x = x.ParenthesisLevelLeft
                    (new[] {PrioTable.BeginOfText}, new[] {PrioTable.EndOfText});

                //Tracer.FlaggedLine("\n"+x.ToString());
                return x;
            }
        }


        public readonly IParser<SourceSyntax> Parser;

        readonly ISubParser<SourceSyntax> _declarationSyntaxSubParser;
        readonly PrioParser<SourceSyntax> _declarationSyntaxParser;

        public MainTokenFactory
            (Func<ITokenFactory<SourceSyntax>, IScanner<SourceSyntax>> getScanner)
        {
            Parser = new PrioParser<SourceSyntax>(PrioTable, getScanner(this));
            _declarationSyntaxParser = new PrioParser<SourceSyntax>
                (
                DeclarationTokenFactory.PrioTable,
                getScanner(new DeclarationTokenFactory())
                );
            _declarationSyntaxSubParser = new SubParser<SourceSyntax>
                (_declarationSyntaxParser, Pack);
        }

        public bool Trace
        {
            get { return Parser.Trace; }
            set
            {
                Parser.Trace = value;
                _declarationSyntaxParser.Trace = value;
            }
        }

        protected override TokenClass SpecialTokenClass(System.Type type)
        {
            if(type == typeof(Exclamation))
                return new Exclamation(_declarationSyntaxSubParser);
            return base.SpecialTokenClass(type);
        }

        static IType<SourceSyntax> Pack(SourceSyntax options) => new SyntaxBoxToken(options);

        protected override TokenClass GetEndOfText() => new EndToken();
        protected override TokenClass GetNumber() => new Number();
        protected override TokenClass GetTokenClass(string name) => new UserSymbol(name);
        protected override TokenClass GetError(Match.IError message) => new SyntaxError(message);
        protected override TokenClass GetText() => new Text();
    }


    sealed class SyntaxError : TokenClass
    {
        readonly IssueId _issue;
        public SyntaxError(IssueId issue) { _issue = issue; }
        public SyntaxError(Match.IError message) { _issue = ReniLexer.Parse(message); }
        protected override Syntax Terminal(SourcePart token)
            => new CompileSyntaxError(_issue, token);
        protected override Syntax Suffix(Syntax left, SourcePart token)
            => left.SyntaxError(_issue, token);
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => left.SyntaxError(_issue, token, right);
        public override string Id => "<error>";

        protected override Syntax Prefix(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }
    }
}