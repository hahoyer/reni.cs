using hw.DebugFormatter;
using hw.UnitTest;

namespace Reni.Runtime.Tests
{
    [UnitTest]
    class Base : DependenceProvider
    {
        [UnitTest]
        void RefBytes()
        {
            {
                var bytes = Runtime.DataHandler.RefBytes;
                (bytes == 8).Assert();
                (sizeof(long) == bytes).Assert();
            }
        }
    }
}