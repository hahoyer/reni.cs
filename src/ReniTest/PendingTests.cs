//16:55:22.808 04.01.2022 ran 185 of 186 

// ReSharper disable once CheckNamespace

using ReniUI.Test.Formatting;

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

            TestRunner.RunTest(new TrainWreck().ComplexHead);

TestRunner.RunTest(new ReniUI.Test.UserInteraction.BigExample().Reformat);

// notrun 

TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
