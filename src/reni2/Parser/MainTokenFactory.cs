using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Numeric;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class MainTokenFactory : TokenFactory
    {
        static PrioTable PrioTable
        {
            get
            {
                var result = PrioTable.Left(PrioTable.Any);
                result += PrioTable.Left
                (
                    AtToken.TokenId,
                    "_N_E_X_T_",
                    ToNumberOfBase.TokenId
                );

                result += PrioTable.Left(ConcatArrays.TokenId, ConcatArrays.MutableId);

                result += PrioTable.Left(Star.TokenId, Slash.TokenId, "\\");
                result += PrioTable.Left(Plus.TokenId, Minus.TokenId);

                result += PrioTable.Left
                (
                    CompareOperation.TokenId(),
                    CompareOperation.TokenId(canBeEqual: true),
                    CompareOperation.TokenId(false),
                    CompareOperation.TokenId(false, true)
                );
                result += PrioTable.Left
                    (EqualityOperation.TokenId(false), EqualityOperation.TokenId());

                result += PrioTable.Right(ReassignToken.TokenId);

                result += PrioTable.Right(ThenToken.TokenId);
                result += PrioTable.Right(ElseToken.TokenId);

                result += PrioTable.Right(Exclamation.TokenId);
                result += PrioTable.Left
                (
                    Function.TokenId(),
                    Function.TokenId(true),
                    Function.TokenId(isMetaFunction: true));
                result += PrioTable.Right(Colon.TokenId);
                result += PrioTable.Right(List.TokenId(0));
                result += PrioTable.Right(List.TokenId(1));
                result += PrioTable.Right(List.TokenId(2));
                result += PrioTable.Right(Cleanup.TokenId);
                result += PrioTable.Right(PrioTable.Error);

                result += PrioTable.BracketParallels
                (
                    new[]
                    {
                        LeftParenthesis.TokenId(3),
                        LeftParenthesis.TokenId(2),
                        LeftParenthesis.TokenId(1),
                        PrioTable.BeginOfText
                    },
                    new[]
                    {
                        RightParenthesisBase.TokenId(3),
                        RightParenthesisBase.TokenId(2),
                        RightParenthesisBase.TokenId(1),
                        PrioTable.EndOfText
                    }
                );

                //Tracer.FlaggedLine("\n"+x.ToString());
                return result;
            }
        }

        public readonly IParser<Syntax> Parser;

        readonly ISubParser<Syntax> _declarationSyntaxSubParser;
        readonly PrioParser<Syntax> _declarationSyntaxParser;
        readonly List<UserSymbol> UserSymbols = new List<UserSymbol>();

        public MainTokenFactory(Func<ITokenFactory, IScanner> getScanner)
        {
            Parser = new PrioParser<Syntax>(PrioTable, getScanner(this), new BeginOfText());
            _declarationSyntaxParser = new PrioParser<Syntax>
            (
                DeclarationTokenFactory.PrioTable,
                getScanner(new DeclarationTokenFactory()),
                null
            );
            _declarationSyntaxSubParser = new SubParser<Syntax>(_declarationSyntaxParser, Pack);
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

        protected override IParserTokenType<Syntax> SpecialTokenClass(System.Type type)
        {
            if(type == typeof(Exclamation))
                return new Exclamation(_declarationSyntaxSubParser);

            return base.SpecialTokenClass(type);
        }

        static IParserTokenType<Syntax> Pack(Syntax options) => new ExclamationBoxToken(options);

        internal override IParserTokenType<Syntax> GetTokenClass(string name)
        {
            var result = new UserSymbol(name);
            UserSymbols.Add(result);
            return result;
        }

        [DisableDump]
        internal IEnumerable<IParserTokenType<Syntax>> AllTokenClasses
            => PredefinedTokenClasses.Concat(UserSymbols);
    }
}