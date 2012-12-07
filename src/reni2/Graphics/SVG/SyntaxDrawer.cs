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

namespace Reni.Graphics.SVG
{
    sealed class SyntaxDrawer : ReniObject, ISyntaxDrawer
    {
        readonly Syntax _syntax;
        int SizeBase { get { return (int) (_tick * 10); } }
        readonly Root _root;
        readonly float _tick;
        readonly string _stroke;
        readonly string _fillColor;
        readonly string _fontFamily;
        readonly Font _font;
        readonly System.Drawing.Graphics _graphics;

        internal SyntaxDrawer(IGraphTarget target)
        {
            _fontFamily = "Arial";
            _stroke = "Black";
            _tick = 1;
            _fillColor = "LightBlue";
            _font = new Font(FontFamily.Families.Single(f1 => f1.Name == _fontFamily), _tick * 10);
            _graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1));

            _root = new Root {Svg = new SVG()};
            _syntax = Syntax.Create(target, this);
        }

        internal Root Draw()
        {
            if(_syntax == null)
                return _root;

            _syntax.Draw(new Point(SizeBase / 2, SizeBase / 2));

            _root.Svg.Width = _syntax.Width + SizeBase + "px";
            _root.Svg.Height = _syntax.Height + SizeBase + "px";

            return _root;
        }


        Size ISyntaxDrawer.Gap { get { return new Size(SizeBase, SizeBase); } }
        int ISyntaxDrawer.NodeHeight(string nodeName) { return Math.Max(TextSize(nodeName).Height, SizeBase) + SizeBase; }
        int ISyntaxDrawer.NodeWidth(string nodeName) { return Math.Max(TextSize(nodeName).Width, SizeBase) + SizeBase; }

        void ISyntaxDrawer.DrawNode(Point origin, string nodeName)
        {
            var size = NodeSize(nodeName);
            var bodyWidth = size.Width - 2 * SizeBase;
            var lineOrigin = new Size(origin.X + SizeBase, origin.Y);

            _root.Svg.Items.Add
                (
                    new Path
                    {
                        PathData = PathElement.Format
                        (
                            new LineElement(lineOrigin, isVisible: false),
                            new HorizontalLineElement(bodyWidth),
                            new ArcElement(SizeBase, new Size(0, SizeBase * 2), false, true),
                            new HorizontalLineElement(-bodyWidth),
                            new ArcElement(SizeBase, new Size(0, -SizeBase * 2), false, true)
                        ),
                        Fill = _fillColor,
                        Stroke = _stroke,
                        StrokeWidth = _tick
                    }
                );

            _root.Svg.Items.Add(CreateText(nodeName, origin + new Size(size.Width / 2, size.Height / 2)));
        }

        Content CreateText(string text, Point center)
        {
            var size = TextSize(text);
            var start = center + new Size(-size.Width / 2, size.Height / 2);
            return new Text
            {
                Data = text,
                Start = start,
                FontFamily = _font.FontFamily.Name,
                Size = (float) (_font.Size * 1.4),
                Fill = _stroke
            };
        }

        void ISyntaxDrawer.DrawLine(Point start, Point end) { _root.Svg.Items.Add(CreateLine(start, end)); }

        Content CreateLine(Point start, Point end)
        {
            return new Path
            {
                PathData = PathElement.Format
                    (
                        new LineElement(new Size(start), isVisible: false),
                        new LineElement(new Size(end.X - start.X, end.Y - start.Y))
                    ),
                Stroke = _stroke,
                StrokeWidth = _tick
            };
        }

        Size TextSize(string nodeName)
        {
            var result = _graphics.MeasureString(nodeName, _font);
            return new Size((int) result.Width, (int) result.Height);
        }

        Size NodeSize(string nodeName)
        {
            var textSize = TextSize(nodeName);
            return new Size
                (Math.Max(textSize.Width, SizeBase) + SizeBase
                 , SizeBase * 2);
        }
    }
}