using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using ReniUI.CompilationView;

namespace ReniUI
{
    static class Extension
    {
        const int DefaultTextSize = 10;
        static readonly Control _dummy = new Control();

        internal static Control CreateGroup(this Control client, string title)
        {
            var result = new GroupBox
            {
                Text = title,
                AutoSize = true,
                Dock = DockStyle.Fill,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            client.Location = result.DisplayRectangle.Location;

            result.Controls.Add(client);
            return result;
        }

        internal static Control CreateColumnView(this IEnumerable<Control> controls)
            => InternalCreateLineupView(true, controls);

        internal static Control CreateRowView(this IEnumerable<Control> controls)
            => InternalCreateLineupView(false, controls);

        internal static Control CreateLineupView
            (this bool inColumns, params Control[] controls)
            => InternalCreateLineupView(inColumns, controls);

        internal static TableLayoutPanel ForceLineupView
            (this bool inColumns, params Control[] controls)
            => CreateTableLayoutPanel(inColumns, controls);

        static Control InternalCreateLineupView
            (bool useColumns, IEnumerable<Control> controls)
        {
            var effectiveControls = controls
                .Where(item => item != null && item != _dummy)
                .ToArray();
            return effectiveControls.Length == 1
                ? effectiveControls[0]
                : CreateTableLayoutPanel(useColumns, effectiveControls);
        }

        static TableLayoutPanel CreateTableLayoutPanel(bool inColumns, Control[] controls)
        {
            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = inColumns ? controls.Length : 1,
                RowCount = inColumns ? 1 : controls.Length,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            result.Controls.AddRange(controls);
            return result;
        }

        internal static Control CreateView(this Dumpable dumpable)
            => CreateView(dumpable.Dump());

        internal static Label CreateView(this string text, double factor = 1, bool isBold = false)
            => new Label
            {
                Font = CreateFont(factor, isBold),
                AutoSize = true,
                Text = text
            };

        static Font CreateFont(double factor, bool isBold = false)
            =>
            new Font
            (
                "Lucida Console",
                (int) (DefaultTextSize * factor),
                isBold ? FontStyle.Bold : FontStyle.Regular
            );

        internal static Label CreateView(this int value, double factor = 1, bool isBold = false)
            => new Label
            {
                Font = CreateFont(factor, isBold),
                AutoSize = true,
                Text = value.ToString(),
                TextAlign = ContentAlignment.MiddleRight
            };

        internal static Label CreateView(this Reni.Basics.Size size)
            => size == null
                ? "unknown size".CreateView()
                : size.ToInt().CreateView();

        public interface IClickHandler
        {
            void Signal(object target);
        }

        internal static Control CreateLink(this object target, IClickHandler master)
        {
            var result = target.GetIdText().CreateView();
            result.Click += (s, a) => master.Signal(target);
            return result;
        }

        internal static string FilePosition(this SourcePart sourcePart)
        {
            var source = sourcePart.Source;
            var position = sourcePart.Position;
            var positionEnd = sourcePart.EndPosition;
            return source.Identifier + "(" +
                (source.LineIndex(position) + 1) + "," +
                (source.ColumnIndex(position) + 1) + "," +
                (source.LineIndex(positionEnd) + 1) + "," +
                (source.ColumnIndex(positionEnd) + 1) + ")";
        }

        internal static IEnumerable<T> Query<T>(Func<IEnumerable<T>> function)
            => new QueryClass<T>(function);

        sealed class QueryClass<T> : IEnumerable<T>
        {
            readonly Func<IEnumerable<T>> Function;
            public QueryClass(Func<IEnumerable<T>> function) { Function = function; }
            IEnumerator<T> GetEnumerator() => Function().GetEnumerator();
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

    }

}