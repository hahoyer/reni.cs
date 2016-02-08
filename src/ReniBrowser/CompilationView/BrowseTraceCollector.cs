using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using Reni;
using Reni.Code;

namespace ReniBrowser.CompilationView
{
    sealed class BrowseTraceCollector : DumpableObject, ITraceCollector
    {
        internal sealed class Step : DumpableObject
        {
            internal class StackItem
            {
                public int FunctionIndex;
                public int ReturnStepIndex;
                public string Frame;
            }

            internal readonly int Index;
            internal readonly IFormalCodeItem CodeBase;
            internal readonly DataStack.ItemMemento[] Before;
            internal Exception Exception;
            internal DataStack.ItemMemento[] After;
            internal StackItem[] Stack;

            internal Step(IFormalCodeItem codeBase, DataStack.ItemMemento[] before, int index)
            {
                CodeBase = codeBase;
                Before = before;
                Index = index;
            }

            internal DataGridViewRow CreateRowForStep()
            {
                var result = new DataGridViewRow();
                result.Cells.Add
                    (
                        new DataGridViewTextBoxCell
                        {
                            Value = Index
                        });
                result.Cells.Add
                    (
                        new DataGridViewTextBoxCell
                        {
                            Value = CodeBase.NodeDump()
                        });
                if(Exception != null)
                    result.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        BackColor = Color.Red
                    };
                return result;
            }

            internal static IEnumerable<DataGridViewColumn> DataGridViewColumns()
            {
                yield return new DataGridViewTextBoxColumn
                {
                    Name = "#",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                };
                yield return new DataGridViewTextBoxColumn
                {
                    Name = "Code",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };
            }

            internal void OnSelect(int columnIndex, SourceView master)
            {
                if(columnIndex == 0)
                    master.SignalClickedStep(this);

                if(columnIndex == 1)
                    master.SignalClickedCode(CodeBase);
            }

            internal Control CreateView(SourceView master)
                => false.CreateLineupView
                    (
                        Exception?.ToString().CreateView(),
                        CreateDataStackView(),
                        CreateStackView(master)
                    );

            Control CreateDataStackView()
            {
                var data = Before
                    .Merge(After, item => item.Offset)
                    .OrderByDescending(item=>item.Item1)
                    .ToArray();

                var result = new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 5,
                    RowCount = data.Length,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
                };

                for(var i = 0; i < data.Length; i++)
                {
                    var item = data[i];
                    var isChange = item.Item2?.ValueDump != item.Item3?.ValueDump;

                    if(item.Item2 != null)
                    {
                        result.Controls.Add(item.Item2.Size.CreateView(isBold: isChange), 0, i);
                        result.Controls.Add(item.Item2.ValueDump.CreateView(isBold: isChange), 1, i);
                    }

                    result.Controls.Add(item.Item1.CreateView(isBold: isChange), 2, i);

                    if (item.Item3 != null)
                    {
                        result.Controls.Add(item.Item3.Size.CreateView(isBold: isChange), 3, i);
                        result.Controls.Add(item.Item3.ValueDump.CreateView(isBold: isChange), 4, i);

                    }
                }

                return result;
            }
            Control CreateStackView(SourceView master)
            {
                if (Stack == null)
                    return null;

                var result = new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 3,
                    RowCount = Stack.Length,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
                };

                for (var i = 0; i < Stack.Length; i++)
                {
                    var item = Stack[i];
                    result.Controls.Add(item.FunctionIndex.CreateView(), 0, i);
                    result.Controls.Add(item.ReturnStepIndex.CreateView(), 1, i);
                    result.Controls.Add(item.Frame.CreateView(), 2, i);
                }

                return result;
            }
        }

        readonly IList<Step> Steps = new List<Step>();
        readonly IDictionary<IFormalCodeItem, int[]> StepsForCode =
            new Dictionary<IFormalCodeItem, int[]>();

        void ITraceCollector.AssertionFailed(Func<string> dumper, int depth)
        {
            throw new AssertionFailedException(dumper());
        }

        void ITraceCollector.Run(DataStack dataStack, IFormalCodeItem codeBase)
        {
            var beforeSize = dataStack.Size;
            var index = Steps.Count;
            var item = new Step(codeBase, dataStack.GetLocalItemMementos().ToArray(), index);
            Steps.Add(item);
            AssumeStepsForCode(item.CodeBase, index);

            try
            {
                codeBase.Visit(dataStack);
            }
            catch(Exception exception)
            {
                item.Exception = exception;
                dataStack.Size = beforeSize + codeBase.Size;
            }

            item.After = dataStack.GetLocalItemMementos().ToArray();
        }

        void AssumeStepsForCode(IFormalCodeItem item, int index)
        {
            if(!StepsForCode.ContainsKey(item))
                StepsForCode.Add(item, new int[0]);
            StepsForCode[item] = StepsForCode[item].Concat(new[] {index}).ToArray();
        }

        internal abstract class RunException : Exception
        {
            protected RunException(string message)
                : base(message) {}
        }

        internal sealed class AssertionFailedException : RunException
        {
            public AssertionFailedException(string message)
                : base(message) {}
        }

        internal ITraceLogItem GetItems(IFormalCodeItem target, SourceView master)
        {
            if(!StepsForCode.ContainsKey(target))
                return EmptyItem;
            var data = StepsForCode[target].Select(item => Steps[item]);
            return new TraceLogItem(data, master);
        }

        static readonly ITraceLogItem EmptyItem = new EmptyTraceLogItem();

        internal DataGridView CreateView(SourceView master)
        {
            var result = new DataGridView
            {
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.RowHeaderSelect,
                MultiSelect = true
            };
            result.Columns.AddRange(Step.DataGridViewColumns().ToArray());
            result.Rows.AddRange(Steps.Select(item => item.CreateRowForStep()).ToArray());
            result.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            result.CellClick += (a, b) => Steps[b.RowIndex].OnSelect(b.ColumnIndex, master);
            return result;
        }
    }

    sealed class EmptyTraceLogItem : DumpableObject, ITraceLogItem
    {
        Control ITraceLogItem.CreateLink() => "".CreateView();
    }

    sealed class TraceLogItem : DumpableObject, ITraceLogItem
    {
        readonly SourceView Master;
        readonly BrowseTraceCollector.Step[] Data;

        public TraceLogItem(IEnumerable<BrowseTraceCollector.Step> data, SourceView master)
        {
            Master = master;
            Data = data.ToArray();
        }

        Control ITraceLogItem.CreateLink()
        {
            if(!Data.Any())
                return new Control();

            var text = Data.First().Index.ToString();
            if(Data.Skip(1).Any())
                text += " ...";
            var result = text.CreateView();
            result.Click += (a, b) => OnClick();
            return result;
        }

        void OnClick()
        {
            if(Data.Length == 1)
                Master.SignalClickedStep(Data[0]);
            else
                Master.SignalClickedSteps(Data);
        }
    }
}