using hw.DebugFormatter;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Context : DumpableObject
    {
        sealed class RootContext : Context
        {
            public RootContext(Configuration configuration)
                : base(configuration) {}

            [DisableDump]
            public override Context None => this;

            [DisableDump]
            public override Context LeftSideOfRightParenthesis => new LineBreakContextForLeftParenthesis(Configuration);

            [DisableDump]
            protected override Context ToRoot => this;

            [DisableDump]
            public override Context BodyOfColon => new LineBreakContextForParenthesisAfterColon(Configuration);

            [DisableDump]
            public override Context ForList => new LineBreakContextForList(Configuration);
        }

        sealed class LineBreakContextForParenthesisAfterColon : Context
        {
            public LineBreakContextForParenthesisAfterColon(Configuration configuration)
                : base(configuration) {}

            [DisableDump]
            public override Context LeftSideOfRightParenthesis => new LineBreakContextForLeftParenthesis
                (Configuration, true);
        }

        sealed class LineBreakContextForLeftParenthesis : Context
        {
            public LineBreakContextForLeftParenthesis
                (Configuration configuration, bool lineBreakBeforeLeftParenthesis = false)
                : base(configuration) => LineBreakBeforeLeftParenthesis = lineBreakBeforeLeftParenthesis;

            public override Context ForList => new LineBreakContextForList(Configuration);
            public override bool LineBreaksForLeftParenthesis => true;
            public override bool LineBreakBeforeLeftParenthesis {get;}
        }

        sealed class LineBreakContextForList : Context
        {
            public LineBreakContextForList(Configuration configuration)
                : base(configuration) {}

            public override bool LineBreaksForList => true;

            public override Context MultiLineBreaksForList(BinaryTreeSyntax left, BinaryTreeSyntax right)
                => new MultiLineBreaksForListContext(Configuration, left, right);
        }

        sealed class MultiLineBreaksForListContext : Context
        {
            readonly BinaryTreeSyntax Left;
            readonly BinaryTreeSyntax Right;

            public MultiLineBreaksForListContext(Configuration configuration, BinaryTreeSyntax left, BinaryTreeSyntax right)
                : base(configuration)
            {
                Left = left;
                Right = right;
            }

        }

        sealed class NoneContext : Context
        {
            public NoneContext(Configuration configuration)
                : base(configuration) {}

            [DisableDump]
            public override Context None => this;
        }

        public static Context GetRoot(Configuration configuration) => new RootContext(configuration);

        internal readonly Configuration Configuration;

        Context(Configuration configuration) => Configuration = configuration;

        [DisableDump]
        public virtual Context None => new NoneContext(Configuration);

        [DisableDump]
        public virtual Context BodyOfColon => this;

        [DisableDump]
        public virtual Context ForList => this;

        [DisableDump]
        public virtual Context LeftSideOfRightParenthesis => this;

        public virtual bool LineBreakBeforeLeftParenthesis => false;
        public virtual bool LineBreaksForLeftParenthesis => false;
        public virtual bool LineBreaksForRightParenthesis => false;
        public virtual bool LineBreaksForList => false;
        public virtual bool HasMultipleLineBreaksOnRightSide => false;

        public virtual Context MultiLineBreaksForList(BinaryTreeSyntax left, BinaryTreeSyntax right) => this;

        public Context ChildContext(BinaryTreeSyntax target)
        {
            switch(target.TokenClass)
            {
                case BeginOfText _:
                case EndOfText _: return ToRoot;
                case List _:
                    return ToList;

                default:
                    NotImplementedMethod(target);
                    return null;
            }
        }


        class ListContext : Context
        {
            readonly Context Parent;

            public ListContext(Context parent)
                : base(parent.Configuration)
                => Parent = parent;
        }

        protected virtual Context ToList => new ListContext(this);
        protected virtual Context ToRoot => GetRoot(Configuration);
    }
}