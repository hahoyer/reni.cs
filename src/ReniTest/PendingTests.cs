//18:20:41.460 08.01.2022 ran 185 of 186 

// ReSharper disable once CheckNamespace
namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.UserInteraction.BigExample().Reformat);

// notrun 

TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
