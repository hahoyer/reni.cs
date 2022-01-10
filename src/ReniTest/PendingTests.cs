//20:43:27.641 09.01.2022 ran 183 of 186 

// ReSharper disable once CheckNamespace
namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SeparatorBeforeComment1);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SeparatorBeforeComment2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SeparatorBeforeComment3);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SeparatorBeforeComment4);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().IndentWithLineComment);
TestRunner.RunTest(new ReniUI.Test.Formatting.WithComments().ReformatComments);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.StrangeExpressions().StringPrefix);

// notrun 

TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
