using System;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using Reni;
using Reni.FeatureTest;

namespace ReniTest
{
    public partial class Main : Form
    {
        private Compiler _compiler;

        public Main()
        {
            InitializeComponent();
            CreateCompiler(Struct.AccessEx1Text);
            Service.Connect(treeView1, _compiler);
        }

        private void CreateCompiler(string text)
        {
            var fileName = "temptest.reni";
            var f = File.m(fileName);
            f.String = text;
            _compiler = new Compiler(fileName);
            //_compiler.Exec();
        }
    }
}