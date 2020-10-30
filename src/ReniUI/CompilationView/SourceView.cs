using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.Type;
using ScintillaNET;

namespace ReniUI.CompilationView
{
    public sealed class SourceView : MainView, Extension.IClickHandler
    {
        class CacheContainer
        {
            public FunctionCache<object, ChildView> ChildViews;
            public ValueCache<CompilerBrowser> Compiler;
            public int LineNumberMarginLength;
            public FunctionCache<ValueSyntax, ResultCachesView> ResultCachesViews;
        }

        readonly CacheContainer Cache = new CacheContainer();

        readonly TraceLogView LogView;
        readonly Scintilla TextBox;
        readonly BrowseTraceCollector TraceLog;

        public SourceView(string text)
            : base("SourceView")
        {
            TextBox = new Scintilla
            {
                Lexer = Lexer.Container, VirtualSpaceOptions = VirtualSpace.UserAccessible
            };

            foreach(var id in TextStyle.All)
                StyleConfig(id);

            TextBox.StyleNeeded += (s, args) => Compiler.SignalStyleNeeded(TextBox, args.Position);
            TextBox.TextChanged += (s, args) => OnTextChanged();

            TextBox.ContextMenu = new ContextMenu();
            TextBox.ContextMenu.Popup += (s, args) => OnContextMenuPopup();

            Cache.Compiler = new ValueCache<CompilerBrowser>(CreateCompilerBrowser);
            Cache.ResultCachesViews
                = new FunctionCache<ValueSyntax, ResultCachesView>(item => new ResultCachesView(item, this));
            Cache.ChildViews = new FunctionCache<object, ChildView>(CreateView);

            Client = TextBox;

            TextBox.Text = text;

            TraceLog = new BrowseTraceCollector(this);
            LogView = new TraceLogView(this);
        }

        internal CompilerBrowser Compiler => Cache.Compiler.Value;

        int LineNumberMarginLength
        {
            get => Cache.LineNumberMarginLength;
            set
            {
                if(Cache.LineNumberMarginLength == value)
                    return;

                Cache.LineNumberMarginLength = value;
                const int Padding = 2;
                TextBox.Margins[0].Width
                    = TextBox.TextWidth(Style.LineNumber, new string('9', value + 1)) + Padding;
            }
        }

        void Extension.IClickHandler.Signal(object target) => SignalClickedObject(target);

        CompilerBrowser CreateCompilerBrowser()
            => CompilerBrowser.FromText
            (
                TextBox.Text,
                new CompilerParameters
                {
                    OutStream = new StringStream()
                }
            );

        public new void Run()
        {
            LogView.Run();
            base.Run();
        }

        FunctionView CreateView(FunctionType target)
            => target == null? null : new FunctionView(target, this);

        ContextView CreateView(ContextBase target)
            => target == null? null : new ContextView(target, this);

        TypeView CreateView(TypeBase target)
            => target == null? null : new TypeView(target, this);

        CodeView CreateView(CodeBase target)
            => target == null? null : new CodeView(target, this);

        CompoundView CreateView(Compound target)
            => target == null? null : new CompoundView(target, this);

        StepView CreateView(Step target)
            => target == null? null : new StepView(target, this);

        ChildView CreateView(object target)
            => CreateView(target as FunctionType) ??
               CreateView(target as ContextBase) ??
               CreateView(target as TypeBase) ??
               CreateView(target as CodeBase) ??
               CreateView(target as Step) ?? CreateView(target as Compound) ?? UnexpectedTarget(target);

        ChildView UnexpectedTarget(object item)
        {
            NotImplementedMethod(item);
            return null;
        }

        void OnTextChanged()
        {
            LineNumberMarginLength = TextBox.Lines.Count.ToString().Length;
            Cache.Compiler.IsValid = false;
        }

        void OnContextMenuPopup()
        {
            var menuItems = TextBox.ContextMenu.MenuItems;

            while(menuItems.Count > 0)
                menuItems.RemoveAt(0);

            Compiler.Ensure();

            var p = TextBox.CurrentPosition;

            var compileSyntaxList = Compiler.FindPosition(p);
            var items = compileSyntaxList
                .Select(CreateMenuItem)
                .ToArray();

            menuItems.AddRange(items);
        }

        MenuItem CreateMenuItem(ValueSyntax syntax)
        {
            var text = syntax.GetType().PrettyName() + " " + syntax.ObjectId;

            if(syntax.ResultCache.Count > 1)
                text += " (" + syntax.ResultCache.Count + ")";

            var menuItem = new MenuItem
                (text, (s, a) => Cache.ResultCachesViews[syntax].Run());
            menuItem.Select += (s, a) => SignalContextMenuSelect(syntax);
            return menuItem;
        }

        void SignalContextMenuSelect(ValueSyntax syntax)
            => TextBox.SetSelection(syntax.Anchor.SourceParts.First().Position
                , syntax.Anchor.SourceParts.Last().EndPosition);

        void StyleConfig(TextStyle id) => id.Config(TextBox.Styles[id]);

        void SignalClickedObject(object target) => Cache.ChildViews[target].Run();

        internal void SignalClickedFunction(int index)
            => SignalClickedObject(Compiler.Function(index));

        internal void SelectSource(SourcePart[] source)
        {
            if(source == null)
                return;
            (source.Length == 1).Assert();
            TextBox.SetSelection(source.First().Position, source.First().EndPosition);
        }

        internal ITraceLogItem TraceLogItem(IFormalCodeItem target)
            => TraceLog.GetItems(target);

        internal void RunCode()
        {
            Compiler.Ensure();
            var parent = Compiler.ExecutionContext;
            var dataStack = new DataStack(parent)
            {
                TraceCollector = TraceLog
            };

            Compiler.Execute(dataStack);
        }

        internal DataGridView CreateTraceLogView() => TraceLog.CreateView();

        internal void SignalClickedSteps(Step[] target)
            => LogView.SignalClickedObject(target);

        internal void SignalClickedStep(Step target)
            => SignalClickedObject(target);
    }

    interface ITraceLogItem
    {
        Control CreateLink();
    }
}