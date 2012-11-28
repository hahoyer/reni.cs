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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Graphics
{
    sealed class SyntaxDrawer : ReniObject
    {
        readonly System.Drawing.Graphics _graphics;
        readonly Font _font;
        readonly SolidBrush _lineBrush;
        readonly SolidBrush _nodeBrush;
        readonly Bitmap _bitmap;
        readonly int _sizeBase;
        readonly StringFormat _stringFormat;
        readonly Pen _linePen;

        SyntaxDrawer(IGraphTarget syntax)
        {
            _stringFormat = new StringFormat(StringFormatFlags.NoWrap);
            _font = new Font(FontFamily.Families.Single(f1 => f1.Name == "Arial"), 10);
            _lineBrush = new SolidBrush(Color.Black);
            _linePen = new Pen(_lineBrush, 1);
            _nodeBrush = new SolidBrush(Color.LightBlue);
            _graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1));
            _sizeBase = (_font.Height * 8) / 10;

            var width = Width(syntax) + _sizeBase + 1;
            var height = Height(syntax) + _sizeBase + 1;
            _bitmap = new Bitmap(width, height);
            _graphics = System.Drawing.Graphics.FromImage(_bitmap);
            var frame = new Rectangle(0, 0, width, height);
            _graphics.FillRectangle(new SolidBrush(Color.Transparent), frame);
            _graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        internal static Image DrawBitmap(IGraphTarget syntax)
        {
            var drawer = new SyntaxDrawer(syntax);
            drawer.BuildBitmap(new Point(drawer._sizeBase / 2, drawer._sizeBase / 2), syntax);
            return drawer.Bitmap;
        }

        internal Syntax Create(IGraphTarget syntax) { return syntax == null ? null : new Syntax(this, syntax.Title, syntax.Children); }

        internal Size Gap { get { return new Size(_sizeBase, _sizeBase); } }
        Image Bitmap { get { return _bitmap; } }

        internal void DrawNode(Point origin, string nodeName)
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

        internal void DrawLine(Point start, Point end) { _graphics.DrawLine(_linePen, start, end); }

        int TextWidth(string nodeName)
        {
            return (int)
                   _graphics
                       .MeasureString(nodeName, _font, new PointF(0, 0), _stringFormat)
                       .Width;
        }

        Size NodeSize(string nodeName) { return new Size(NodeWidth(nodeName), NodeHeight(nodeName)); }
        internal int NodeHeight(string nodeName) { return _sizeBase * 2; }
        internal int NodeWidth(string nodeName) { return Math.Max(TextWidth(nodeName), _sizeBase) + _sizeBase; }
        void BuildBitmap(Point point, IGraphTarget syntax) { Create(syntax).Draw(point); }
        int Height(IGraphTarget syntax) { return Create(syntax).Height; }
        int Width(IGraphTarget syntax) { return Create(syntax).Width; }
    }
}