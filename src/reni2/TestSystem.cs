using System.Collections.Generic;
using System.IO;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using NUnit.Framework;

namespace HWFileSystem
{
    [TestFixture]
    public class TestSystem
    {
        [Test]
        public void First()
        {
            Config config = new Config();
            config.Add(new FileSource(@"c:"));
            config["Data"].Add(new FileSource(@"z:\disks\11.11\Data"));
            config["Data"].Add(new FileSource(@"z:\disks\Juri\Data"));
            config["Data"].Add(new FileSource(@"z:\disks\KinoKino\Data"));
            config["Data"].Add(new FileSource(@"z:\disks\NotschnoiDozor\Data"));
            config["Data"].Add(new FileSource(@"z:\disks\Walkuere\Data"));

            Directory d = new Directory(config);
            Tracer.FlaggedLine(d.DebuggerDumpString);
            Tracer.FlaggedLine(d.Files["Data"].DebuggerDumpString);
        }
    }
}
