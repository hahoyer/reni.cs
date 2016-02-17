using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Graphics;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.ParserTest
{
    public static class Services
    {
        public static PrioTable FormatPrioTable(this string text) => PrioTable.FromText(text);

        public static Image SyntaxGraph(this PrioTable prioTable, string code)
        {
            var syntax = Parse(prioTable, code);
            return syntax == null ? new Bitmap(1, 1) : SyntaxDrawer.DrawBitmap(syntax);
        }

        public static IGraphTarget Parse(this PrioTable prioTable, string code)
        {
            if(string.IsNullOrEmpty(code))
                return null;

            IParser<Syntax> parser = new PrioParser<Syntax>
                (prioTable, new Scanner<Syntax>(Lexer.Instance, new SimpleTokenFactory()), null);
            parser.Trace = true;
            return parser.Execute(new Source(code) + 0);
        }

        public static string ToBase64(this Image image)
        {
            var ic = new ImageConverter();
            Tracer.Assert(image != null, () => "image != null");
            var convertTo = (byte[]) ic.ConvertTo(image, typeof(byte[]));
            Tracer.Assert(convertTo != null, () => "convertTo != null");
            return Convert.ToBase64String(convertTo, Base64FormattingOptions.InsertLineBreaks);
        }

        public static Image SaveToFile(this Image image, string fileName)
        {
            image.Save(fileName);
            return image;
        }

        internal abstract class TokenClass
            : DumpableObject, IType<Syntax>, IUniqueIdProvider, Scanner<Syntax>.IType
        {
            readonly string Id;

            protected TokenClass(string id) { Id = id; }

            Syntax IType<Syntax>.Create(Syntax left, IToken token, Syntax right)
                => Create(left, token, right);

            string IType<Syntax>.PrioTableId => Id;

            protected virtual Syntax Create(Syntax left, IToken token, Syntax right)
                => Syntax.CreateSyntax(left, token, right);

            string IUniqueIdProvider.Value => Id;

            ISubParser<Syntax> Scanner<Syntax>.IType.NextParser => null;
            IType<Syntax> Scanner<Syntax>.IType.Type => this;
        }

        internal abstract class Syntax : DumpableObject, IGraphTarget, IIconKeyProvider, ISourcePart
        {
            [EnableDump]
            protected readonly IToken Token;

            protected Syntax(IToken token) { Token = token; }

            [DisableDump]
            string IIconKeyProvider.IconKey => "Syntax";

            public string Title => Token.Id;

            [DisableDump]
            public IGraphTarget[] Children => new[] {Left, Right};

            protected virtual IGraphTarget Right => null;
            protected virtual IGraphTarget Left => null;

            internal static Syntax CreateSyntax(Syntax left, IToken token, Syntax right)
            {
                if(left == null)
                {
                    if(right == null)
                        return new TerminalSyntax(token);
                    return new PrefixSyntax(token, right);
                }

                if(right == null)
                    return new SuffixSyntax(left, token);
                return new InfixSyntax(left, token, right);
            }

            sealed class InfixSyntax : Syntax
            {
                readonly Syntax _left;
                readonly Syntax _right;

                public InfixSyntax(Syntax left, IToken token, Syntax right)
                    : base(token)
                {
                    _left = left;
                    _right = right;
                }

                protected override IGraphTarget Right => _right;
                protected override IGraphTarget Left => _left;
            }

            sealed class SuffixSyntax : Syntax
            {
                readonly Syntax _left;

                public SuffixSyntax(Syntax left, IToken token)
                    : base(token) { _left = left; }

                protected override IGraphTarget Left => _left;
            }

            sealed class PrefixSyntax : Syntax
            {
                readonly Syntax _right;

                public PrefixSyntax(IToken token, Syntax right)
                    : base(token) { _right = right; }

                protected override IGraphTarget Right => _right;
            }

            sealed class TerminalSyntax : Syntax
            {
                public TerminalSyntax(IToken token)
                    : base(token) { }

                internal override Syntax ParenthesisMatch(IToken token, Syntax argument)
                    => CreateSyntax(null, Token, argument);
            }

            internal virtual Syntax Match(int level, IToken token)
                => new InfixSyntax(this, token, null);

            internal virtual Syntax ParenthesisMatch(IToken token, Syntax argument)
                => CreateSyntax(this, token, argument);

            SourcePart ISourcePart.All { get { return Token.SourcePart; } }
        }
    }
}