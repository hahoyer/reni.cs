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
                (Configuration, lineBreakBeforeLeftParenthesis: true);
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

            public override Context MultiLineBreaksForList(Helper.Syntax left, Helper.Syntax right)
                => new MultiLineBreaksForListContext(Configuration, left, right);
        }

        sealed class MultiLineBreaksForListContext : Context
        {
            readonly Helper.Syntax Left;
            readonly Helper.Syntax Right;

            public MultiLineBreaksForListContext(Configuration configuration, Helper.Syntax left, Helper.Syntax right)
                : base(configuration)
            {
                Left = left;
                Right = right;
            }

            [DisableDump]
            public override bool HasMultipleLineBreaksOnRightSide
                => Configuration.IsLineBreakRequired(Left) ||
                   Configuration.IsLineBreakRequired(Right);
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

        public virtual Context MultiLineBreaksForList(Helper.Syntax left, Helper.Syntax right) => this;
    }
}