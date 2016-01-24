using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.Type;

namespace ReniBrowser.CompilationView
{
    static class ViewExtension
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

        internal static Control CreateView(this CodeBase code, SourceView master)
        {
            var control = code.Visit(new CodeViewVisitor(master));
            var title = code.Size.ToInt() + " " + code.GetType().Name + "." + code.ObjectId + "i";
            return CreateGroup(control, title);
        }

        internal static Control CreateView(this CodeArgs exts, SourceView master)
            => false.CreateLineupView
                (
                    exts
                        .Data
                        .Select(item => item.CreateLink(master))
                        .ToArray()
                );

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

        internal static Control CreateView(this Type target)
            => target.PrettyName().CreateView();

        internal static Control CreateView(this FunctionInstance target, SourceView master)
            => target.BodyCode.CreateView(master);

        internal static Label CreateView(this string text, double factor = 1)
            => new Label
            {
                Font = CreateFont(factor),
                AutoSize = true,
                Text = text
            };

        static Font CreateFont(double factor)
            => new Font("Lucida Console", (int) (DefaultTextSize * factor));

        internal static Label CreateView(this int value, double factor = 1)
            => new Label
            {
                Font = CreateFont(factor),
                AutoSize = true,
                Text = value.ToString(),
                TextAlign = ContentAlignment.MiddleRight
            };

        internal static Label CreateView(this Reni.Basics.Size size)
            => size == null
                ? "unknown size".CreateView()
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

        internal static string GetIdText(this object target)
        {
            var result = target.GetType().PrettyName();
            if(target is Dumpable)
                return result + "." + target.GetObjectId() + "i";
            return result;
        }

        internal static Control CreateView(this Call target, SourceView master)
        {
            var functionId = target.FunctionId;
            var name = functionId.ToString();
            var result = name.CreateView();
            result.Click += (a, b) => master.SignalClickedFunction(functionId.Index);
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

        internal static Control CreateClient
            (this FunctionCache<ContextBase, ResultCache> target, SourceView master)
            =>
                target.Count == 1
                    ? target.First().CreateView(master)
                    : new ResultCachesViewsPanel(target, master).Client;

        internal static TableLayoutPanel CreateView
            (this KeyValuePair<ContextBase, ResultCache> item, SourceView master)
        {
            var control = false.ForceLineupView
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

            var chidType = item as IChild<TypeBase>;
            if(chidType != null)
                return GetSource(chidType.Parent);

            var hollowChildContext = item as IChild<ContextBase>;
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
                ?? (target as IChild<TypeBase>)?.Parent
                    ?? (object) (target as IChild<ContextBase>)?.Parent;

        internal static Control CreateView(this object target, SourceView master)
            => target
                .ParentChain()
                .Select(item => item.CreateChildView(master))
                .CreateRowView();

        static Control CreateChildView(this object target, SourceView master)
        {
            var head = true.CreateLineupView
                (GetSize(target)?.CreateView(), target.CreateLink(master));

            var childView = CreateChildView(target as FunctionBodyType)
                ?? CreateChildView(target as FunctionType, master)
                    ?? CreateChildView(target as CompoundContext, master)
                        ?? CreateChildView(target as Compound, master)
                            ?? CreateChildView(target as CompoundType)
                                ?? CreateChildView(target as ArrayType)
                                    ?? CreateChildView(target as ArrayReferenceType)
                                        ?? CreateChildView(target as PointerType)
                                            ?? CreateChildView
                                                (target as Reni.Context.Function, master)
                                                ?? CreateChildView(target as Root)
                                                    ?? CreateChildView(target as BitType)
                                                        ?? NotImplemented(target);
            return false.CreateLineupView(head, childView);
        }

        static Reni.Basics.Size GetSize(object target) => (target as TypeBase)?.Size;

        static Control CreateChildView(this FunctionBodyType target) 
            => target?.Syntax.CreateView();

        static Control CreateChildView(this FunctionType target, SourceView master)
        {
            if(target == null)
                return null;

            var indexView = target.Index.CreateView(2);
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

        static Control CreateChildView(this PointerType target)
            => target == null ? null : _dummy;

        static Control CreateChildView(this Root target)
            => target == null ? null : _dummy;

        static Control CreateChildView(this BitType target)
            => target == null ? null : _dummy;

        static Control CreateChildView(this Reni.Context.Function target, SourceView master)
            => target?.ArgsType.CreateLink(master).CreateGroup("Args");

        static Control CreateChildView(CompoundType target)
            => target == null ? null : _dummy;

        static Control CreateChildView(this CompoundContext target, SourceView master)
            => target?.View.CreateView(master);

        static Control CreateChildView(this Compound target, SourceView master)
            => target?.CompoundView.CreateView(master);

        static Control CreateChildView(this ArrayType target)
        {
            if(target == null)
                return null;

            var c = ("count=" + target.Count).CreateView();
            var m = target.IsMutable ? "mutable".CreateView() : null;
            var t = target.IsTextItem ? "text_item".CreateView() : null;

            var result = true.ForceLineupView(c, m, t);
            result.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            return result;
        }

        static Control CreateChildView(this ArrayReferenceType target)
            => target?.DumpOptions.CreateView();

        static Control CreateView(this Reni.Struct.CompoundView target, SourceView master)
            => target.Compound.CreateView(target.ViewPosition, master);

        static Control CreateView(this Compound compound, int viewPosition, SourceView master)
        {
            var x = compound
                .CachedResults
                .Select(i => i.Data.CreateView(master))
                .ToArray();

            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = x.Length,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for(var i = 0; i < x.Length; i++)
            {
                var control = ((i < viewPosition ? "" : "?") + i).CreateView(1.5);
                var sourcePart = compound.Syntax.Statements[i].SourcePart;
                control.Click += (a, b) => master.SelectSource(sourcePart);
                result.Controls.Add(control, 0, i);
                result.Controls.Add(x[i], 1, i);
            }

            return result;
        }

        static Control NotImplemented(this object item)
        {
            Dumpable.NotImplementedFunction(item);
            return null;
        }
    }
}