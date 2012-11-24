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

namespace Reni.Parser
{
    sealed class SyntaxDrawer
    {
        readonly Graphics _graphics;
        readonly Font _font;
        readonly SolidBrush _brush;

        public SyntaxDrawer(Size imageSize)
        {
            _graphics = Graphics.FromImage(new Bitmap(imageSize.Width, imageSize.Height));
            var frame = new Rectangle(0, 0, imageSize.Width, imageSize.Height);
            _graphics.FillRectangle(new SolidBrush(Color.Transparent), frame);
            _font = new Font("Arial", 16);
            _brush = new SolidBrush(Color.Blue);
        }

        static readonly StringFormat _stringFormat = new StringFormat(StringFormatFlags.NoWrap);
        public int MinChildrenShift { get { return _font.Height; } }

        public void Node(Point origin, string nodeName)
        {
            var pen = new Pen(_brush);
            var size = NodeSize(nodeName);
            _graphics.DrawArc(pen, 0, 0, 2 * _font.Height, 2 * _font.Height, 180, 360);
            _graphics.DrawArc(pen, size.Width - 2 * _font.Height, 0, size.Width, 2 * _font.Height, 0, 180);
            _graphics.DrawLine(pen, _font.Height, 0, size.Width - _font.Height, 0);
            _graphics.DrawLine(pen, _font.Height, 2 * _font.Height, size.Width - _font.Height, 2 * _font.Height);
            _graphics.DrawString
                (nodeName
                 , _font
                 , _brush
                 , origin + new Size(_font.Height / 2, _font.Height / 2)
                 , _stringFormat
                );
        }

        public Size NodeSize(string nodeName)
        {
            return new Size
                (NodeWidth(nodeName)
                 , NodeHeight(nodeName)
                );
        }
        internal int NodeHeight(string nodeName) { return _font.Height * 2; }
        internal int NodeWidth(string nodeName) { return (int) _graphics.MeasureString(nodeName, _font, new PointF(0, 0), _stringFormat).Width + _font.Height; }
    }
}