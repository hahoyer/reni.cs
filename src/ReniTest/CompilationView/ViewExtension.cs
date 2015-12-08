using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Helper;
using Reni;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Type;

namespace ReniTest.CompilationView
{
    static class ViewExtension
    {
        internal static Control CreateGroup(this Control client, string title)
        {
            var result = new GroupBox
            {
                Text = title,
                AutoSize = true,
                Dock = DockStyle.Top,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            client.Location = result.DisplayRectangle.Location;

            result.Controls.Add(client);
            return result;
        }

        internal static Control CreateView(this CodeBase code, SourceView master)
        {
            var control = code.Visit(new CodeViewVisitor(master));
            if(!(control is TableLayoutPanel))
                control = true.CreateLineupView(code.GetType().PrettyName().CreateView(), control);
            return CreateGroup(control, "Code");
        }

        internal static Control CreateView(this CodeArgs exts)
            => false.CreateLineupView
                (
                    exts
                        .Data
                        .Select(item => CreateView(item.NodeDump()))
                        .Cast<Control>()
                        .ToArray());

        internal static TableLayoutPanel CreateColumnView(this IEnumerable<Control> controls)
            => InternalCreateLineupView(true, controls);

        internal static TableLayoutPanel CreateRowView(this IEnumerable<Control> controls)
            => InternalCreateLineupView(false, controls);

        internal static TableLayoutPanel CreateLineupView
            (this bool inColumns, params Control[] controls)
            => InternalCreateLineupView(inColumns, controls);

        static TableLayoutPanel InternalCreateLineupView
            (bool inColumns, IEnumerable<Control> controls)
        {
            var effectiveControls = controls.Where(item => item != null).ToArray();

            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = inColumns ? effectiveControls.Length : 1,
                RowCount = inColumns ? 1 : effectiveControls.Length,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            result.Controls.AddRange(effectiveControls);
            return result;
        }

        internal static Control CreateView(this Dumpable dumpable)
            => CreateView(dumpable.Dump());

        internal static Control CreateView(this FunctionInstance target, SourceView master)
            => target.ResultCache.Data.CreateView(master);

        internal static Label CreateView(this string text)
            => new Label
            {
                Font = new Font("Lucida Console", 10),
                AutoSize = true,
                Text = text
            };

        internal static Label CreateView(this int value)
            => new Label
            {
                Font = new Font("Lucida Console", 10),
                AutoSize = true,
                Text = value.ToString(),
                TextAlign = ContentAlignment.MiddleRight
            };

        internal static Label CreateView(this Reni.Basics.Size size)
            => size == null
                ? "unknown size".CreateView()
                : size.IsZero
                    ? "hollow".CreateView()
                    : size.ToInt().CreateView();

        internal static Control CreateSizeView(this Result result)
        {
            var size = result.Type?.Size
                ?? result.Size ?? (result.HasHllw ? Reni.Basics.Size.Zero : null);
            return size.CreateView();
        }

        internal static Control CreateView(this TypeBase type, SourceView master)
        {
            if(type == null)
                return "".CreateView();
            var text = type.NodeDump.CreateView();
            text.Click += (s, a) => master.SignalClicked(type);
            return text;
        }

        internal static Control CreateView(this FunctionType item, SourceView master)
        {
            var indexView = new Label
            {
                Font = new Font("Lucida Console", 20),
                AutoSize = true,
                Text = item.Index.ToString()  ,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var compoundView = item.FindRecentCompoundView.CompoundContext.CreateView(master);
            var argsTypeView = item.ArgsType.CreateView(master);

            var header = true
                .CreateLineupView(indexView, false.CreateLineupView(compoundView, argsTypeView));

            var setterView = item.Setter?.CreateView(master);
            var getterView = item.Getter?.CreateView(master);

            return false.CreateLineupView
                (
                    header,
                    setterView?.CreateGroup("Set"),
                    getterView?.CreateGroup("Get")
                );
        }

        internal static Control CreateView(this ContextBase context, SourceView master)
        {
            var result = context.NodeDump.CreateView();
            result.Click += (s, a) => master.SignalClicked(context);
            return result;
        }

        internal static Control CreateView(this Call visitedObject, SourceView master)
        {
            var functionId = visitedObject.FunctionId;
            var name = functionId.ToString();
            var result = name.CreateView();
            var menuItem = new MenuItem(name, (a, b) => master.SignalClicked(functionId));
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(menuItem);
            result.ContextMenu = contextMenu;
            return result;
        }

        internal static Control CreateTypeLineView(this Result result, SourceView master)
        {
            if(!result.HasType && !result.HasSize && !result.HasHllw)
                return null;

            return true.CreateLineupView(result.CreateSizeView(), result.Type.CreateView(master));
        }

        internal static Control CreateTypeLineView(this TypeBase target, SourceView master)
            => true.CreateLineupView(target.Size.CreateView(), target.CreateView(master));

        internal static TableLayoutPanel CreateClient(this CompileSyntax syntax, SourceView master)
        {                                 
            var resultCacheViews =
                syntax
                    .ResultCache
                    .Select(item => CreateView(item, master))
                    .ToArray();

            var client = new TableLayoutPanel
            {
                ColumnCount = resultCacheViews.Length,
                RowCount = 1
            };

            var styles = resultCacheViews
                .Select
                (
                    item => new ColumnStyle(SizeType.Percent)
                    {
                        Width = 100 / resultCacheViews.Length
                    }
                );

            foreach(var item in styles)
                client.ColumnStyles.Add(item);

            client.Controls.AddRange(resultCacheViews);
            return client;
        }

        internal static Control CreateView
            (this KeyValuePair<ContextBase, ResultCache> item, SourceView master)
        {
            var control = false.CreateLineupView
                (
                    item.Key.CreateView(master),
                    item.Value.Data.CreateView(master)
                )
                ;
            control.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;
            return control;
        }

        internal static Control CreateView(this Result result, SourceView master)
        {
            var clients = new List<Control>();
            if(result.HasType || result.HasSize || result.HasHllw)
                clients.Add(result.CreateTypeLineView(master));
            clients.Add(result.Exts?.CreateView());
            clients.Add(result.Code?.CreateView(master));
            return clients.CreateRowView();
        }
    }
}