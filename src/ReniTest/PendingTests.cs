//01:30:54.260 02.01.2022 ran 180 of 184 

// ReSharper disable once CheckNamespace
namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().HalfList);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.Formatting.BadThings().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.Formatting.WithComments().ReformatComments);

// notrun 

TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
