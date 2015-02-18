using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Forms;
using hw.Graphics;
using hw.Parser;
using hw.Scanner;

namespace Reni.Parser
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
                (prioTable, new Scanner<Syntax>(ReniLexer.Instance), new SimpleTokenFactory());
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

        internal abstract class TokenClass : DumpableObject, IType<Syntax>, INameProvider
        {
            string _name;
            Syntax IType<Syntax>.Create(Syntax left, SourcePart part, Syntax right) => Create(left, part, right);
            string IType<Syntax>.PrioTableName => _name;
            ISubParser<Syntax> IType<Syntax>.NextParser => null;
            IType<Syntax> IType<Syntax>.NextTypeIfMatched => NextTypeIfMatched;
            virtual internal IType<Syntax> NextTypeIfMatched => null;
            protected abstract Syntax Create(Syntax left, SourcePart token, Syntax right);
            string INameProvider.Name { set { _name = value; } }
        }

        internal abstract class Syntax : DumpableObject, IGraphTarget, IIconKeyProvider
        {
            [EnableDump]
            readonly SourcePart _token;

            protected Syntax(SourcePart token) { _token = token; }

            [DisableDump]
            string IIconKeyProvider.IconKey => "Syntax";

            [DisableDump]
            protected virtual SourcePart FirstToken => _token;

            [DisableDump]
            internal virtual SourcePart LastToken => _token;

            public string Title => _token.Name;

            [DisableDump]
            public IGraphTarget[] Children => new[] {Left, Right};

            protected virtual IGraphTarget Right => null;
            protected virtual IGraphTarget Left => null;

            internal static Syntax CreateSyntax(Syntax left, SourcePart token, Syntax right)
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
                public InfixSyntax(Syntax left, SourcePart token, Syntax right)
                    : base(token)
                {
                    _left = left;
                    _right = right;
                }
                protected override SourcePart FirstToken => _left.FirstToken;
                internal override SourcePart LastToken => _right.LastToken;
                protected override IGraphTarget Right => _right;
                protected override IGraphTarget Left => _left;
            }

            sealed class SuffixSyntax : Syntax
            {
                readonly Syntax _left;
                public SuffixSyntax(Syntax left, SourcePart token)
                    : base(token)
                {
                    _left = left;
                }
                protected override SourcePart FirstToken => _left.FirstToken;
                protected override IGraphTarget Left => _left;
            }

            sealed class PrefixSyntax : Syntax
            {
                readonly Syntax _right;
                public PrefixSyntax(SourcePart token, Syntax right)
                    : base(token)
                {
                    _right = right;
                }
                internal override SourcePart LastToken => _right.LastToken;
                protected override IGraphTarget Right => _right;
            }

            sealed class TerminalSyntax : Syntax
            {
                public TerminalSyntax(SourcePart token)
                    : base(token)
                {}
                internal override Syntax ParenthesisMatch(SourcePart token, Syntax argument) => CreateSyntax(null, FirstToken, argument);
            }

            internal virtual Syntax Match(int level, SourcePart token) => new InfixSyntax(this, token, null);
            internal virtual Syntax ParenthesisMatch(SourcePart token, Syntax argument) => CreateSyntax(this, token, argument);
        }
    }
}