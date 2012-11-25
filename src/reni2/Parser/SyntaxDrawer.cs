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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    sealed class SyntaxDrawer : ReniObject
    {
        readonly Graphics _graphics;
        readonly Font _font;
        readonly SolidBrush _lineBrush;
        readonly SolidBrush _nodeBrush;
        readonly Bitmap _bitmap;
        readonly int _sizeBase;
        readonly StringFormat _stringFormat;
        readonly Pen _linePen;

        SyntaxDrawer(IParsedSyntax syntax)
        {
            _stringFormat = new StringFormat(StringFormatFlags.NoWrap);
            _font = new Font(FontFamily.Families.Single(f1 => f1.Name == "Arial"), 16);
            _lineBrush = new SolidBrush(Color.Black);
            _linePen = new Pen(_lineBrush, 1);
            _nodeBrush = new SolidBrush(Color.LightBlue);
            _graphics = Graphics.FromImage(new Bitmap(1, 1));
            _sizeBase = (_font.Height * 8) / 10;

            var width = Width(syntax) + _sizeBase + 1;
            var height = Height(syntax) + _sizeBase + 1;
            _bitmap = new Bitmap(width, height);
            _graphics = Graphics.FromImage(_bitmap);
            var frame = new Rectangle(0, 0, width, height);
            _graphics.FillRectangle(new SolidBrush(Color.Transparent), frame);
            _graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        internal static Image DrawBitmap(IParsedSyntax syntax)
        {
            var drawer = new SyntaxDrawer(syntax);
            drawer.BuildBitmap(new Point(drawer._sizeBase / 2, drawer._sizeBase / 2), syntax);
            return drawer.Bitmap;
        }

        Syntax Create(IParsedSyntax syntax)
        {
            return syntax.Left == null
                       ? (
                             syntax.Right == null
                                 ? (Syntax) new Terminal(syntax.Token.Name, this)
                                 : new Prefix(syntax.Token.Name, syntax.Right, this)
                         )
                       : (
                             syntax.Right == null
                                 ? (Syntax) new Suffix(syntax.Left, syntax.Token.Name, this)
                                 : new Infix(syntax.Left, syntax.Token.Name, syntax.Right, this)
                         );
        }

        int MinChildrenShift { get { return _sizeBase; } }
        Size Gap { get { return new Size(_sizeBase, _sizeBase); } }
        Image Bitmap { get { return _bitmap; } }

        void DrawNode(Point origin, string nodeName)
        {
            var size = NodeSize(nodeName);
            var arcSize = new Size(2 * _sizeBase, 2 * _sizeBase);
            var bodyWidth = new Size(size.Width - 2 * _sizeBase, 0);
            var lineOrigin = origin + new Size(_sizeBase, 0);

            var r = new GraphicsPath();
            r.AddArc(new Rectangle(origin, arcSize), 90, 180);
            r.AddLine(lineOrigin, lineOrigin + bodyWidth);
            r.AddArc(new Rectangle(origin + bodyWidth, arcSize), 270, 180);
            r.AddLine(lineOrigin + bodyWidth + new Size(0, _sizeBase * 2), lineOrigin + new Size(0, _sizeBase * 2));
            _graphics.FillPath(_nodeBrush, r);
            _graphics.DrawPath(_linePen, r);

            var s = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            _graphics.DrawString(nodeName, _font, _lineBrush, new Rectangle(origin, size), s);
        }

        void DrawLine(Point start, Point end) { _graphics.DrawLine(_linePen, start, end); }

        int TextWidth(string nodeName)
        {
            return (int)
                   _graphics
                       .MeasureString(nodeName, _font, new PointF(0, 0), _stringFormat)
                       .Width;
        }

        Size NodeSize(string nodeName) { return new Size(NodeWidth(nodeName), NodeHeight(nodeName)); }
        int NodeHeight(string nodeName) { return _sizeBase * 2; }
        int NodeWidth(string nodeName) { return Math.Max(TextWidth(nodeName), _sizeBase) + _sizeBase; }
        void BuildBitmap(Point point, IParsedSyntax syntax) { Create(syntax).Draw(point); }
        int Height(IParsedSyntax syntax) { return Create(syntax).Height; }
        int Width(IParsedSyntax syntax) { return Create(syntax).Width; }

        abstract class Syntax : ReniObject
        {
            readonly string _name;
            protected readonly SyntaxDrawer Drawer;

            protected Syntax(string name, SyntaxDrawer drawer)
            {
                _name = name;
                Drawer = drawer;
            }
            internal abstract int Height { get; }
            internal abstract int Width { get; }
            internal abstract void Draw(Point origin);

            protected void DrawNode(Point origin) { Drawer.DrawNode(origin, _name); }
            protected int NodeHeight { get { return Drawer.NodeHeight(_name); } }
            protected int NodeWidth { get { return Drawer.NodeWidth(_name); } }
            protected abstract Size Anchor { get; }
            protected int OneChildWidth(int width)
            {
                var nw = NodeWidth;
                var halfDelta = (width - nw) / 2;
                return nw
                       + Math.Max(0, halfDelta - Drawer.MinChildrenShift)
                       + Math.Max(0, halfDelta + Drawer.MinChildrenShift);
            }
            protected int NonTerminalHeight(int height) { return NodeHeight + Drawer.Gap.Height + height; }
            protected Size CalculateAchor(Syntax syntax, int factor)
            {
                return new Size
                    (Math.Max(NodeWidth / 2, syntax.Width / 2 + factor * Drawer.MinChildrenShift)
                     , NodeHeight / 2
                    );
            }

            protected void Draw(Point origin, Syntax syntax, int factor)
            {
                var rawNodeOffset = (syntax.Width - NodeWidth) / 2 + factor * Drawer.MinChildrenShift;
                
                var nodeOffset = Math.Max(0, rawNodeOffset);
                var nodeShift = new Size(nodeOffset, 0);
                
                var childOffset = -Math.Min(0, rawNodeOffset);
                var childShift = new Size(childOffset, NodeHeight + Drawer.Gap.Height);
                
                Drawer.DrawLine(origin + Anchor, origin + childShift + syntax.Anchor);
                DrawNode(origin + nodeShift);
                syntax.Draw(origin + childShift);
            }

            protected void Draw(Point origin, Syntax left, Syntax right)
            {
                var rawNodeOffset = (InfixWidth(left, right) - NodeWidth) / 2;

                var nodeOffset = Math.Max(0, rawNodeOffset);
                var nodeShift = new Size(nodeOffset, 0);

                var leftOffset = -Math.Min(0, rawNodeOffset);
                var leftShift = new Size(leftOffset, NodeHeight + Drawer.Gap.Height);

                var rightOffset = leftOffset + left.Width + Drawer.Gap.Width;
                var rightShift = new Size(rightOffset, NodeHeight + Drawer.Gap.Height);
                
                Drawer.DrawLine(origin + Anchor, origin + leftShift + left.Anchor);
                Drawer.DrawLine(origin + Anchor, origin + rightShift + right.Anchor);
                DrawNode(origin + nodeShift);
                left.Draw(origin + leftShift);
                right.Draw(origin + rightShift);
            }

            protected int InfixWidth(Syntax left, Syntax right)
            {
                var nw = NodeWidth;
                var cw = left.Width + Drawer.Gap.Width + right.Width;
                return Math.Max(nw, cw);
            }
        }

        sealed class Infix : Syntax
        {
            readonly Syntax _left;
            readonly Syntax _right;
            public Infix(IParsedSyntax left, string name, IParsedSyntax right, SyntaxDrawer drawer)
                : base(name, drawer)
            {
                _left = drawer.Create(left);
                _right = drawer.Create(right);
            }
            internal override int Height
            {
                get
                {
                    return
                        NodeHeight
                        + Math.Max(_left.Height, _right.Height)
                        + Drawer.Gap.Height;
                }
            }
            internal override int Width { get { return InfixWidth(_left, _right); } }
            internal override void Draw(Point origin) { Draw(origin, _left, _right); }
            protected override Size Anchor { get { return new Size(Width / 2, NodeHeight / 2); } }
        }

        sealed class Suffix : Syntax
        {
            readonly Syntax _left;
            public Suffix(IParsedSyntax left, string name, SyntaxDrawer drawer)
                : base(name, drawer) { _left = drawer.Create(left); }
            internal override int Height { get { return NonTerminalHeight(_left.Height); } }
            internal override int Width { get { return OneChildWidth(_left.Width); } }
            internal override void Draw(Point origin) { Draw(origin, _left, 1); }
            protected override Size Anchor { get { return CalculateAchor(_left, 1); } }
        }

        sealed class Prefix : Syntax
        {
            readonly Syntax _right;
            public Prefix(string name, IParsedSyntax right, SyntaxDrawer drawer)
                : base(name, drawer) { _right = drawer.Create(right); }
            internal override int Height { get { return NonTerminalHeight(_right.Height); } }
            internal override int Width { get { return OneChildWidth(_right.Width); } }
            internal override void Draw(Point origin) { Draw(origin, _right, -1); }
            protected override Size Anchor { get { return CalculateAchor(_right, -1); } }
        }

        sealed class Terminal : Syntax
        {
            public Terminal(string name, SyntaxDrawer drawer)
                : base(name, drawer) { }
            internal override int Height { get { return NodeHeight; } }
            internal override int Width { get { return NodeWidth; } }
            internal override void Draw(Point origin) { DrawNode(origin); }
            protected override Size Anchor { get { return new Size(NodeWidth / 2, NodeHeight / 2); } }
        }
    }
}