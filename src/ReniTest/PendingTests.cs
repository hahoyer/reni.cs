//23:03:24.886 23.10.2020 ran 168 of 178 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {

            TestRunner.RunTest(new ReniUI.Test.PairedSyntaxTree().SimpleList);
            TestRunner.RunTest(new ReniUI.Test.PairedSyntaxTree().TopLevelList);
            TestRunner.RunTest(new ReniUI.Test.PairedSyntaxTree().List);
        }
    }
}
