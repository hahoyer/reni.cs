using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Formatting;
using Reni.Numeric;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
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

        protected override ScannerTokenClass SpecialTokenClass(System.Type type)
        {
            if(type == typeof(Exclamation))
                return new Exclamation(_declarationSyntaxSubParser);
            return base.SpecialTokenClass(type);
        }

        static IType<SourceSyntax> Pack(SourceSyntax options) => new ExclamationBoxToken(options);

        protected override ScannerTokenClass GetEndOfText() => new EndToken();
        protected override ScannerTokenClass GetNumber() => new Number();
        protected override ScannerTokenClass GetTokenClass(string name) => new UserSymbol(name);

        protected override ScannerTokenClass GetError(Match.IError message)
            => new ScannerSyntaxError(message);

        protected override ScannerTokenClass GetText() => new Text();
    }


    sealed class ScannerSyntaxError : ScannerTokenClass, IType<SourceSyntax>, ITokenClass
    {
        readonly IssueId _issue;

        public ScannerSyntaxError(Match.IError message)
        {
            _issue = Lexer.Parse(message);
            StopByObjectIds(81);
        }

        string IType<SourceSyntax>.PrioTableId => Id;

        public override string Id => "<error>";

        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
        {
            var resultIssues = _issue.CreateIssue(token.Characters);
            return new SourceSyntax
                (
                left,
                this,
                token,
                right,
                ListSyntax.Create(left?.Syntax, right?.Syntax),
                new[] {resultIssues});
        }

        IType<SourceSyntax> IType<SourceSyntax>.NextTypeIfMatched => null;

        internal Formatting.Item ItemFromToken
            (Provider provider, int leadingLineBreaks, int indentLevel, ITokenClass rightTokenClass, IToken token)
        {
            var whiteSpaces = provider.InternalGetWhitespaces
                (
                    rightTokenClass,
                    leadingLineBreaks,
                    indentLevel,
                    token.PrecededWith,
                    this
                );
            var item = new Formatting.Item(whiteSpaces, token);
            return item;
        }
    }
}