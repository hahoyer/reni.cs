using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
    public class Control
    {
        public bool InvokeRequired;
        public DockStyle Dock;
        public void Invoke(Action action);
        public List<Control> Controls;
        public void SuspendLayout() { throw new NotImplementedException(this); }
        public void ResumeLayout() { throw new NotImplementedException(this); }
    }

    enum DockStyle
    {
        Fill
    }

    delegate void PopupEventHandler(object sender, PopupEventArgs e);

    class PopupEventArgs { }

    class MenuItem
    {
        public bool Enabled;
        public MenuItem(string name, PopupEventHandler handler = null) => throw new NotImplementedException();

        public List<MenuItem> MenuItems => throw new NotImplementedException();

        public event PopupEventHandler Popup = (sender, args) => { };
        public event EventHandler Select;    }

    class DataGridView : Control
    {
        public bool RowHeadersVisible;
        public DataGridViewSelectionMode SelectionMode;
        public bool MultiSelect;
        public List<DataGridViewColumn> Columns;
        public List<DataGridViewRow> Rows;

        public void AutoResizeColumns
            (DataGridViewAutoSizeColumnsMode allCells) => throw new NotImplementedException(this);

        public event DataGridViewCellEventHandler CellClick;
    }

    delegate void DataGridViewCellEventHandler(object sender, DataGridViewCellEventHandlerArgs args);

    class DataGridViewCellEventHandlerArgs
    {
        public int RowIndex;
        public int ColumnIndex;
    }

    enum DataGridViewAutoSizeColumnsMode
    {
        AllCells
    }

    public class DataGridViewColumn { }

    public class DataGridViewRow
    {
        public class DataGridViewCell { }

        public List<DataGridViewCell> Cells;
        public DataGridViewCellStyle DefaultCellStyle;
        public bool Selected;
        public bool Visible;
    }

    enum DataGridViewSelectionMode
    {
        RowHeaderSelect
    }

    class OpenFileDialog:Control
    {
        public string Title;
        public bool RestoreDirectory;
        public string InitialDirectory;
        public string FileName;
        public string Filter;
        public bool CheckFileExists;
        public int FilterIndex;
        public string DefaultExt;
        public DialogResult ShowDialog() => throw new NotImplementedException();
    }

    enum DialogResult
    {
        OK
    }

    public class Form:Control 
    {
        public bool MaximizeBox;
        public string Name;
        public string Text;
        public bool Visible;
        public bool MinimizeBox;
        public event EventHandler<CancelEventArgs> Closing;
        public void Show() => throw new NotImplementedException(this);
        public void BringToFront() => throw new NotImplementedException(this);
        public event EventHandler Activated;
    }

    public class Label : Control
    {
        public event EventHandler Click;
    }
    public class TableLayoutPanel : Control
    {
        public bool AutoSize;
        public AutoSizeMode AutoSizeMode;
        public int ColumnCount;
        public int RowCount;
        public TableLayoutPanelCellBorderStyle CellBorderStyle;
        public ControlList Controls;
    }

    public class ControlList
    {
        public void Add(Control control, int p1, int p2) { throw new NotImplementedException(this); }
    }

    
    enum AutoSizeMode
    {
        GrowAndShrink
    }

    enum TableLayoutPanelCellBorderStyle
    {
        Single
        , OutsetPartial
        , None
    }
    internal class DataGridViewCellStyle
    {
        public Color BackColor;
        public DataGridViewContentAlignment Alignment;
    }

    internal class DataGridViewTextBoxCell :DataGridViewRow.DataGridViewCell
    {
        public object Value;
    }

    internal class DataGridViewTextBoxColumn : DataGridViewColumn
    {
        public string Name;
        public DataGridViewAutoSizeColumnMode AutoSizeMode;
        public DataGridViewCellStyle DefaultCellStyle;
    }

    internal enum DataGridViewContentAlignment
    {
        MiddleRight
    }

    internal enum DataGridViewAutoSizeColumnMode
    {
        AllCells
    }

}