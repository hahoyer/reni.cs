#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;

namespace Reni.Parser
{
    public static class Services
    {
        public static PrioTable FormatPrioTable(string text) { return PrioTable.FromText(text); }

        public static Image SyntaxGraph(PrioTable prioTable, string code, Size imageSize)
        {
            if(code == null)
                return new Bitmap(1, 1);

            var parser = new ParserInst(new ReniScanner(), new TokenFactory(prioTable, imageSize));
            ((Syntax) parser.Compile(new Source(code))).Draw(new Point(0, 0));
            return new Bitmap(imageSize.Width, imageSize.Height);
        }

        public static string ToBase64(this Image image)
        {
            var ic = new ImageConverter();
            Tracer.Assert(image != null, () => "image != null");
            var convertTo = (byte[]) ic.ConvertTo(image, typeof(byte[]));
            Tracer.Assert(convertTo != null, () => "convertTo != null");
            return Convert.ToBase64String(convertTo, Base64FormattingOptions.InsertLineBreaks);
        }

        sealed class TokenFactory : TokenFactory<TokenClass>
        {
            readonly SyntaxDrawer _syntaxDrawer;
            readonly PrioTable _prioTable;
            public TokenFactory(PrioTable prioTable, Size imageSize)
            {
                _prioTable = prioTable;
                _syntaxDrawer = new SyntaxDrawer(imageSize);
            }

            protected override TokenClass GetSyntaxError(string message) { throw new Exception("Syntax error: " + message); }
            protected override PrioTable GetPrioTable() { return _prioTable; }
            protected override DictionaryEx<string, TokenClass> GetTokenClasses() { return new DictionaryEx<string, TokenClass>(); }
            protected override TokenClass GetEndOfTextClass() { return CommonTokenClass; }
            protected override TokenClass GetBeginOfTextClass() { return CommonTokenClass; }
            protected override TokenClass GetNewTokenClass(string name) { return CommonTokenClass; }
            protected override TokenClass GetNumberClass() { return CommonTokenClass; }
            protected override TokenClass GetTextClass() { return CommonTokenClass; }
            TokenClass CommonTokenClass { get { return new TokenClass(_syntaxDrawer); } }
        }

        internal abstract class Syntax : ReniObject, IParsedSyntax
        {
            [EnableDump]
            readonly TokenData _token;
            readonly TokenClass _tokenClass;
            readonly SimpleCache<Size> _sizeCache;

            protected Syntax(TokenData token, TokenClass tokenClass)
            {
                _sizeCache = new SimpleCache<Size>(() => new Size(Width, Height));
                _token = token;
                _tokenClass = tokenClass;
            }

            [DisableDump]
            string IIconKeyProvider.IconKey { get { return "Syntax"; } }
            [DisableDump]
            TokenData IParsedSyntax.Token { get { return Token; } }
            [DisableDump]
            TokenData Token { get { return _token; } }
            [DisableDump]
            TokenData IParsedSyntax.FirstToken { get { return FirstToken; } }
            [DisableDump]
            TokenData IParsedSyntax.LastToken { get { return LastToken; } }
            [DisableDump]
            protected abstract TokenData FirstToken { get; }
            [DisableDump]
            protected abstract TokenData LastToken { get; }
            [DisableDump]
            Size Size { get { return _sizeCache.Value; } }
            [DisableDump]
            SyntaxDrawer Drawer { get { return _tokenClass.Drawer; } }

            protected abstract int Width { get; }
            protected abstract int Height { get; }

            string IParsedSyntax.Dump() { return Dump(); }
            string IParsedSyntax.GetNodeDump() { return NodeDump; }

            internal abstract void Draw(Point origin);

            internal static Syntax CreateSyntax(Syntax left, TokenData token, TokenClass tokenClass, Syntax right)
            {
                if(left == null)
                {
                    if(right == null)
                        return new TerminalSyntax(token, tokenClass);
                    return new PrefixSyntax(token, tokenClass, right);
                }

                if(right == null)
                    return new SuffixSyntax(left, token, tokenClass);
                return new InfixSyntax(left, token, tokenClass, right);
            }

            sealed class InfixSyntax : Syntax
            {
                readonly Syntax _left;
                readonly Syntax _right;
                public InfixSyntax(Syntax left, TokenData token, TokenClass tokenClass, Syntax right)
                    : base(token, tokenClass)
                {
                    _left = left;
                    _right = right;
                }
                protected override TokenData FirstToken { get { return _left.FirstToken; } }
                protected override TokenData LastToken { get { return _right.LastToken; } }
                protected override int Width { get { return _left.Size.Width + _right.Size.Width + 100; } }
                protected override int Height { get { return Math.Max(_left.Size.Height, _right.Size.Height) + 100; } }

                internal override void Draw(Point origin) { NotImplementedMethod(origin); }
            }

            sealed class SuffixSyntax : Syntax
            {
                readonly Syntax _left;
                public SuffixSyntax(Syntax left, TokenData token, TokenClass tokenClass)
                    : base(token, tokenClass) { _left = left; }
                protected override TokenData FirstToken { get { return _left.FirstToken; } }
                protected override TokenData LastToken { get { return Token; } }
                protected override int Width
                {
                    get
                    {
                        var nodeWidth = Drawer.NodeWidth(Token.Name);
                        var halfDifference = (_left.Size.Width - nodeWidth) / 2;
                        return nodeWidth
                               + Math.Max(0, halfDifference + Drawer.MinChildrenShift)
                               + Math.Max(0, halfDifference - Drawer.MinChildrenShift);
                    }
                }
                protected override int Height
                {
                    get
                    {
                        NotImplementedMethod();
                        return 0;
                    }
                }
                internal override void Draw(Point origin)
                {
                    var nodeSize = Drawer.NodeSize(Token.Name);

                    Drawer.Node(origin, Token.Name);
                }
            }

            sealed class PrefixSyntax : Syntax
            {
                readonly Syntax _right;
                public PrefixSyntax(TokenData token, TokenClass tokenClass, Syntax right)
                    : base(token, tokenClass) { _right = right; }
                protected override TokenData FirstToken { get { return Token; } }
                protected override TokenData LastToken { get { return _right.LastToken; } }
                protected override int Width
                {
                    get
                    {
                        var nodeWidth = Drawer.NodeWidth(Token.Name);
                        var halfDifference = (_right.Size.Width - nodeWidth) / 2;
                        return nodeWidth
                               + Math.Max(0, halfDifference + Drawer.MinChildrenShift)
                               + Math.Max(0, halfDifference - Drawer.MinChildrenShift);
                    }
                }
                protected override int Height
                {
                    get
                    {
                        NotImplementedMethod();
                        return 0;
                    }
                }
                internal override void Draw(Point origin) { NotImplementedMethod(origin); }
            }

            sealed class TerminalSyntax : Syntax
            {
                public TerminalSyntax(TokenData token, TokenClass tokenClass)
                    : base(token, tokenClass) { }
                protected override TokenData FirstToken { get { return Token; } }
                protected override TokenData LastToken { get { return Token; } }
                protected override int Width { get { return Drawer.NodeWidth(Token.Name); } }
                protected override int Height { get { return Drawer.NodeHeight(Token.Name); } }
                internal override void Draw(Point origin) { Drawer.Node(origin, Token.Name); }
            }
        }

        internal sealed class TokenClass : ReniObject, ITokenClass
        {
            internal readonly SyntaxDrawer Drawer;

            public TokenClass(SyntaxDrawer drawer) { Drawer = drawer; }
            string ITokenClass.Name { get; set; }
            ITokenFactory ITokenClass.NewTokenFactory { get { return null; } }
            IParsedSyntax ITokenClass.Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right)
            {
                return Syntax
                    .CreateSyntax((Syntax) left, token, this, (Syntax) right);
            }
        }
    }
}