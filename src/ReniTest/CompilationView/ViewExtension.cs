using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
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

        internal static Control CreateView(this CodeArgs exts, SourceView master)
            => false.CreateLineupView
                (
                    exts
                        .Data
                        .Select(item => item.CreateLink(master))
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

        internal static Control CheckedCreateView(this TypeBase type, SourceView master)
        {
            if(type == null)
                return "".CreateView();
            return type.CreateLink(master);
        }

        internal static Control CreateLink(this object target, SourceView master)
        {
            var text = target.GetType().PrettyName() + "." + target.GetObjectId() + "i";
            var result = text.CreateView();
            result.Click += (s, a) => master.SignalClicked(target);
            return result;
        }

        internal static Control CreateView(this Call target, SourceView master)
        {
            var functionId = target.FunctionId;
            var name = functionId.ToString();
            var result = name.CreateView();
            result.Click +=
                (a, b) => master.SignalClickedFunction(functionId.Index)
                ;
            return result;
        }

        internal static Control CreateTypeLineView(this Result target, SourceView master)
        {
            if(!target.HasType && !target.HasSize && !target.HasHllw)
                return null;

            return true.CreateLineupView
                (target.CreateSizeView(), target.Type.CheckedCreateView(master));
        }

        internal static Control CreateTypeLineView(this TypeBase target, SourceView master)
            => true.CreateLineupView(target.Size.CreateView(), target.CheckedCreateView(master));

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
                    item.Key.CreateLink(master),
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
            clients.Add(result.Exts?.CreateView(master));
            clients.Add(result.Code?.CreateView(master));
            return clients.CreateRowView();
        }

        internal static SourcePart GetSource(this object item)
        {
            var target = item as ISourceProvider;
            if(target != null)
                return target.Value;

            var chidType = item as IHollowChild<TypeBase>;
            if(chidType != null)
                return GetSource(chidType.Parent);

            var hollowChildContext = item as IHollowChild<ContextBase>;
            if(hollowChildContext != null)
                return GetSource(hollowChildContext.Parent);

            var childContext = item as Child;
            if(childContext != null)
                return GetSource(childContext.Parent);

            return null;
        }

        static IEnumerable<object> ParentChain(this object target)
        {
            var current = target;
            do
            {
                yield return current;
                current = ObtainParent(current);
            } while(current != null);
        }

        static object ObtainParent(object target)
            => (target as Child)?.Parent
                ?? (target as IHollowChild<TypeBase>)?.Parent
                    ?? (object) (target as IHollowChild<ContextBase>)?.Parent;

        internal static Control CreateView(this object target, SourceView master)
            => target
                .ParentChain()
                .Select(item => item.CreateChildView(master))
                .CreateRowView();

        static Control CreateChildView(this object target, SourceView master)
        {
            var childView = CreateChildView(target as FunctionType, master)
                ?? CreateChildView(target as CompoundContext)
                    ?? CreateChildView(target as PointerType, master)
                        ?? CreateChildView(target as Function, master)
                            ?? CreateChildView(target as Root)
                                ?? NotImplemented(target);
            return false.CreateLineupView(target.CreateLink(master), childView);
        }

        static Control CreateChildView(this FunctionType target, SourceView master)
        {
            if(target == null)
                return null;

            var indexView = new Label
            {
                Font = new Font("Lucida Console", 20),
                AutoSize = true,
                Text = target.Index.ToString(),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var argsTypeView = target.ArgsType.CreateLink(master);
            var setterView = target.Setter?.CreateView(master);
            var getterView = target.Getter.CreateView(master);

            return false.CreateLineupView
                (
                    true.CreateLineupView(indexView, argsTypeView),
                    setterView?.CreateGroup("Set"),
                    getterView?.CreateGroup("Get")
                );
        }

        static Control CreateChildView(this IHollowChild<ContextBase> target, SourceView master)
        {
            if(target == null)
                return null;

            return true.CreateLineupView
                (
                    target.GetType().PrettyName().CreateView(),
                    target.Parent.CreateLink(master)
                );
        }

        static Control CreateChildView(this IHollowChild<TypeBase> target, SourceView master)
        {
            if(target == null)
                return null;

            return true.CreateLineupView
                (
                    ((TypeBase) target).Size.CreateView(),
                    target.GetType().PrettyName().CreateView(),
                    target.Parent.CreateLink(master)
                );
        }

        static Control CreateChildView(this PointerType target, SourceView master)
        {
            if(target == null)
                return null;

            return true.CreateLineupView
                (
                    target.Size.CreateView(),
                    target.CreateLink(master)
                );
        }

        static Control CreateChildView(this Root target)
            => target == null ? null : "Root".CreateView();

        static Control CreateChildView(this Function target, SourceView master)
            => target?.ArgsType.CreateLink(master).CreateGroup("Args");

        static Control CreateChildView(this CompoundContext target)
            => target?.View.CreateView();

        static Control NotImplemented(this object item)
        {
            Dumpable.NotImplementedFunction(item);
            return null;
        }
    }
}