//00:01:25.728 26.07.2021 ran 177 of 183 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.TokenLocating().FromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CommentFromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().NamedCompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction().GetTokenForPosition);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().GetTokenForPositionSimple);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction2().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteSimple().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
