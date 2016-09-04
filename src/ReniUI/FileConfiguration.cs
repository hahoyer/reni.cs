using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using ScintillaNET;

namespace ReniUI
{
    sealed class FileConfiguration : DumpableObject
    {
        internal readonly string FileName;
        readonly ValueCache<Persister> FilePersister;

        public FileConfiguration(string fileName)
        {
            FileName = fileName;
            FilePersister = new ValueCache<Persister>
                (() => new Persister(ItemFile("EditorConfiguration")));
        }

        internal string Status
        {
            get { return ItemFile("Status").String; }
            private set { ItemFile("Status").String = value; }
        }

        internal DateTime? LastUsed
        {
            get { return FromDateTime(ItemFile("LastUsed").String); }
            private set { ItemFile("LastUsed").String = value?.ToString("O"); }
        }

        static DateTime? FromDateTime(string value)
        {
            if(value == null)
                return null;

            DateTime result;
            if(DateTime.TryParse(value, out result))
                return result;

            return null;
        }

        internal void ConnectToEditor(Scintilla editor)
        {
            FilePersister.Value.Register
                (
                    "Selection",
                    item => editor.SetSelection(item.Item1, item.Item2),
                    () =>
                        new Tuple<int, int>
                            (editor.SelectionStart, editor.SelectionEnd))
                ;
            FilePersister.Value.Load();
            Status = "Open";
            editor.UpdateUI += (s, args) => OnUpdate(args.Change);
        }

        internal void ConnectToFrame(Form frame)
        {
            frame.FormClosing += (a, s) => OnClosing();
            frame.Activated += (a, s) => LastUsed = DateTime.Now;
        }

        hw.Helper.File ItemFile(string itemName) => ItemFileName(itemName).FileHandle();

        string ItemFileName(string itemName)
            => SystemConfiguration
                .GetConfigurationPath(FileName)
                .PathCombine(itemName);

        void OnClosing() { Status = "Closed"; }
        void OnActivated() { LastUsed = DateTime.Now; }

        void OnUpdate(UpdateChange change)
        {
            if(change.HasFlag(UpdateChange.Selection))
                FilePersister.Value.Store("Selection");
        }

        internal string PositionPath => ItemFileName("Position");
    }
}