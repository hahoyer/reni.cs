//13:32:42.491 01.12.2021 ran 173 of 181 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedLeftParenthesis().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedRightParenthesis().Run);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().MatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().MoreMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().NotMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120TopLineBreak);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120EmptyBrackets);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().TwoLevelParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().UseLineBreakBeforeParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().HalfList);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction().GetTokenForPosition);

// dependanterror 

TestRunner.RunTest(new Reni.FeatureTest.TextConcat().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedBrackets().Run);
TestRunner.RunTest(new ReniUI.Test.Formatting.BadThings().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.Formatting.WithComments().ReformatComments);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().GetTokenForPositionSimple);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction2().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
