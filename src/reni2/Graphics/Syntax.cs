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

namespace Reni.Graphics
{
    sealed class Syntax : ReniObject
    {
        readonly string _name;
        readonly Syntax[] _children;
        readonly SyntaxDrawer _drawer;

        internal Syntax(SyntaxDrawer drawer, string name, IGraphTarget[] children)
        {
            _name = name;
            _drawer = drawer;
            _children = children.Select(drawer.Create).ToArray();
        }

        bool HasChildren { get { return _children.Any(c => c != null); } }
        static int SaveWidth(Syntax syntax) { return syntax == null ? 0 : syntax.Width; }
        static int SaveHeight(Syntax syntax) { return syntax == null ? 0 : syntax.Height; }
        static int SaveAnchorWidth(Syntax syntax) { return syntax == null ? 0 : syntax.Anchor.Width; }

        internal int Height { get { return NodeHeight + (HasChildren ? _drawer.Gap.Height + ChildrenHeight : 0); } }
        internal int Width { get { return Math.Max(NodeWidth, HasChildren ? ChildrenWidth : 0); } }

        int NodeHeight { get { return _drawer.NodeHeight(_name); } }
        int NodeWidth { get { return _drawer.NodeWidth(_name); } }

        Size Anchor { get { return new Size(AnchorOffset, NodeHeight / 2); } }
        int NodeOffset { get { return AnchorOffset - NodeWidth / 2; } }
        int ChildrenOffset { get { return -Math.Min(0, (ChildrenWidth - NodeWidth) / 2); } }

        internal void Draw(Point origin)
        {
            if(HasChildren)
                DrawChildren(origin);
            DrawNode(origin);
        }

        void DrawNode(Point origin) { _drawer.DrawNode(origin + new Size(NodeOffset, 0), _name); }

        void DrawChildren(Point origin)
        {
            var offsets = ChildOffsets.ToArray();
            for(var index = 0; index < _children.Length; index++)
                if(_children[index] != null)
                {
                    _drawer.DrawLine
                        (origin + Anchor
                         , origin + offsets[index] + _children[index].Anchor
                        );
                    _children[index].Draw(origin + offsets[index]);
                }
        }

        IEnumerable<Size> ChildOffsets
        {
            get
            {
                Tracer.Assert(HasChildren);
                var height = NodeHeight + _drawer.Gap.Height;
                var currentWidthOffset = ChildrenOffset;
                foreach(var syntax in _children)
                {
                    yield return new Size(currentWidthOffset, height);
                    currentWidthOffset += SaveWidth(syntax) + _drawer.Gap.Width;
                }
                yield break;
            }
        }

        int ChildrenWidth
        {
            get
            {
                Tracer.Assert(HasChildren);
                var gapWidth = _drawer.Gap.Width * (_children.Length - 1);
                var effectiveChildrenWidth
                    = _children
                        .Select(SaveWidth)
                        .Sum();
                return gapWidth + effectiveChildrenWidth;
            }
        }

        int ChildrenHeight
        {
            get
            {
                Tracer.Assert(HasChildren);
                return _children.Select(SaveHeight).Max();
            }
        }


        int AnchorOffset
        {
            get
            {
                if(!HasChildren)
                    return NodeWidth / 2;

                var childAnchors = _children.Select(SaveAnchorWidth);
                return
                    ChildOffsets
                        .Select((o, i) => o.Width + childAnchors.ElementAt(i))
                        .Sum()
                    / _children.Length;
            }
        }
    }
}