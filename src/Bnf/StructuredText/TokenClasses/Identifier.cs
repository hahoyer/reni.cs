using Bnf.Forms;
using Bnf.Parser;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText.TokenClasses
{
    abstract class Terminal : DumpableObject, ITokenType, IHiearachicalItem<ISyntax>, ILiteral
    {
        ISyntax IHiearachicalItem<ISyntax>.Parse(IParserCursor source, IContext<ISyntax> context)
        {
            var token = context[source];
            return token.Type == this ? CreateMatch(token) : null;
        }

        string IHiearachicalItem<ISyntax>.Name => Name;
        string IUniqueIdProvider.Value => Name;
        protected abstract string Name {get;}

        protected abstract ISyntax CreateMatch(TokenGroup token);
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class Identifier : Terminal
    {
        public Identifier() {}
        protected override string Name => "identifier";
        protected override ISyntax CreateMatch(TokenGroup token) => new Singleton(token);
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class SignedInteger : Terminal
    {
        protected override string Name => "signed_integer";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class BooleanLiteral : Terminal
    {
        protected override string Name => "boolean_literal";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class UnsignedInteger : Terminal
    {
        protected override string Name => "unsigned_integer";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class BinaryInteger : Terminal
    {
        protected override string Name => "binary_integer";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class OctalInteger : Terminal
    {
        protected override string Name => "octal_integer";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class HexInteger : Terminal
    {
        protected override string Name => "hex_integer";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class Real : Terminal
    {
        protected override string Name => "real";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class CharacterString : Terminal
    {
        protected override string Name => "character_string";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class Duration : Terminal
    {
        protected override string Name => "duration";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class TimeOfDay : Terminal
    {
        protected override string Name => "time_of_day";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class Date : Terminal
    {
        protected override string Name => "date";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class DateAndTime : Terminal
    {
        protected override string Name => "date_and_time";

        protected override ISyntax CreateMatch(TokenGroup token)
        {
            NotImplementedMethod(token);
            return new Singleton(token);
        }
    }
}