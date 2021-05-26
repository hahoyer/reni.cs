using hw.UnitTest;

namespace Reni.Runtime.Tests
{
    [UnitTest]
    public class DataHandler: DependenceProvider
    {
        [UnitTest]
        public void Assign()
        {
            var data = Runtime.Data.Create(18);
            data.SizedPush(1, 10);
            data.SizedPush(1, 4);
            data.Push(data.Pointer(1));
            data.Push(data.Pointer(8));
            data.Assign(1);
        }
    }
}