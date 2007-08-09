using HWClassLibrary.Debug;
using NUnit.Framework;

namespace HWFileSystem
{
    [TestFixture]
    public class TestSystem
    {
        [Test]
        public void First()
        {
            Directory d = Directory.CreateMSFileSystem(@"c:", "Data");
            Tracer.FlaggedLine(d.DebuggerDumpString);
        }
    }
}