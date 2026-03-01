namespace Reni.FeatureTest.Helper;

static class Extension
{
    extension(Compiler compiler)
    {
        public void AssertBinaryTreeIsLike(LikeSyntax prototype)
            => prototype.AssertLike(compiler.BinaryTree.Left!.Right);

        public void AssertSyntaxIsLike(LikeSyntax prototype)
            => prototype.AssertLike(compiler.Syntax);
    }
}