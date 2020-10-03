using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class HierarchicalStructure : DumpableObject
    {
        internal class Frame : HierarchicalStructure
        {
            [DisableDump]
            protected override IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
            {
                get
                {
                    Tracer.Assert(Target.Left != null);
                    Tracer.Assert(Target.Left.Left == null);
                    Tracer.Assert(Target.Left.TokenClass is BeginOfText);
                    Tracer.Assert(Target.Left.Right != null);
                    Tracer.Assert(Target.TokenClass is EndOfText);
                    Tracer.Assert(Target.Right == null);

                    return T
                    (
                        CreateChild(Target.Left.Right).Edits,
                        GetWhiteSpacesEdits(Target)
                    );
                }
            }
        }

        class ListFrame : HierarchicalStructure
        {
            public ListFrame() => AdditionalLineBreaksForMultilineItems = true;

            [DisableDump]
            protected override IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
                => GetEditGroupsForChains<List>();
        }

        class ExpressionFrame : HierarchicalStructure
        {
            readonly ITokenClass TokenClass;

            public ExpressionFrame(ITokenClass tokenClass) => TokenClass = tokenClass;

            protected override IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
            {
                get
                {
                    Tracer.Assert(Target.TokenClass == TokenClass, Target.Dump);

                    if(Target.Left != null)
                    {
                        yield return CreateChild(Target.Left).Edits;
                        if(IsLineSplit)
                            yield return T(SourcePartEditExtension.MinimalLineBreak);
                    }

                    yield return GetWhiteSpacesEdits(Target);

                    if(Target.Right != null)
                        yield return CreateChild(Target.Right).Edits;
                }
            }
        }

        class ColonFrame : HierarchicalStructure
        {
            [DisableDump]
            protected override IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
                => GetEditGroupsForChains<Colon>();

            protected override HierarchicalStructure Create(ITokenClass tokenClass, bool isLast)
            {
                var result = base.Create(tokenClass, isLast);
                if(isLast && result is ExpressionFrame)
                    result.IsIndentRequired = true;
                return result;
            }
        }

        class ParenthesisFrame : HierarchicalStructure
        {
            [DisableDump]
            protected override IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
            {
                get
                {
                    Tracer.Assert(Target.Left != null);
                    Tracer.Assert(Target.Left.Left == null);
                    Tracer.Assert(Target.Left.TokenClass is LeftParenthesis);
                    Tracer.Assert(Target.Left.TokenClass.IsBelongingTo(Target.TokenClass));
                    Tracer.Assert(Target.TokenClass is RightParenthesis);
                    Tracer.Assert(Target.Right == null);

                    var isLineSplit = IsLineSplit;

                    yield return GetWhiteSpacesEdits(Target.Left);

                    if(isLineSplit)
                        yield return T(SourcePartEditExtension.MinimalLineBreak);

                    if(Target.Left.Right != null)
                    {
                        var child = CreateChild(Target.Left.Right);
                        child.ForceLineSplit = isLineSplit;
                        yield return child.Edits.Indent(IndentDirection.ToRight);

                        if(isLineSplit)
                            yield return T(SourcePartEditExtension.MinimalLineBreak);
                    }

                    yield return GetWhiteSpacesEdits(Target);
                }
            }
        }

        static bool GetIsSeparatorRequired(Syntax target)
            => !target.WhiteSpaces.HasComment() &&
               SeparatorExtension.Get(target.LeftNeighbor?.TokenClass, target.TokenClass);

        static bool True => true;
        static bool False => false;
        bool AdditionalLineBreaksForMultilineItems;

        internal Configuration Configuration;
        bool ForceLineSplit;

        bool IsIndentRequired;
        internal Syntax Target;

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal IEnumerable<ISourcePartEdit> Edits
        {
            get
            {
                var trace = ObjectId == -240;
                StartMethodDump(trace);
                try
                {
                    var result = EditGroups
                        .SelectMany(i => i)
                        .ToArray()
                        .Indent(IndentDirection);
                    Dump(nameof(result), result);
                    //Tracer.Assert(CheckMultilineExpectations(result), Target.Dump);

                    Tracer.ConditionalBreak(trace);
                    return ReturnMethodDump(result, trace);
                }
                finally
                {
                    EndMethodDump();
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]                               
        IndentDirection IndentDirection => IsIndentRequired ? IndentDirection.ToRight : IndentDirection.NoIndent;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected virtual IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        bool IsLineSplit => ForceLineSplit || GetHasAlreadyLineBreakOrIsTooLong(Target);

        bool CheckMultilineExpectations(IEnumerable<ISourcePartEdit> result)
            => Target.Left == null && Target.Right == null && IsLineSplit ||
               result.Skip(1).Any(edit => edit.HasLines) == IsLineSplit;

        IEnumerable<ISourcePartEdit> GetWhiteSpacesEdits(Syntax target)
        {
            if(target.WhiteSpaces.Any())
                return T(new WhiteSpaceView(target.WhiteSpaces, Configuration, GetIsSeparatorRequired(target)));
            return T(new EmptyWhiteSpaceView(target.Token.Characters.Start, GetIsSeparatorRequired(target)));
        }

        HierarchicalStructure CreateChild(Syntax target, bool isLast = false)
        {
            var child = Create(target.TokenClass, isLast);
            child.Configuration = Configuration;
            child.Target = target;
            return child;
        }

        protected virtual HierarchicalStructure Create(ITokenClass tokenClass, bool isLast)
        {
            switch(tokenClass)
            {
                case List _: return new ListFrame();
                case Colon _: return new ColonFrame();
                case RightParenthesis _: return new ParenthesisFrame();
                default: return new ExpressionFrame(tokenClass);
            }
        }

        bool GetHasAlreadyLineBreakOrIsTooLong(Syntax target)
        {
            var basicLineLength = target.GetFlatLength(Configuration.EmptyLineLimit != 0);
            return basicLineLength == null || basicLineLength > Configuration.MaxLineLength;
        }

        IEnumerable<IEnumerable<ISourcePartEdit>> GetEditGroupsForChains<TSeparator>()
            where TSeparator : ITokenClass
            => Target
                .Chain(target => target.Right)
                .TakeWhile(target => target.TokenClass is TSeparator)
                .Select(GetEdits<TSeparator>)
                .SelectMany(i => i);

        IEnumerable<IEnumerable<ISourcePartEdit>> GetEdits<TSeparator>(Syntax target)
            where TSeparator : ITokenClass
        {
            yield return CreateChild(target.Left).Edits;
            yield return GetWhiteSpacesEdits(target);

            if(target.Right == null)
                yield break;
            
            if(IsLineSplit)
                yield return T(GetLineSplitter(target, target.Right?.TokenClass is TSeparator));

            if(!(target.Right.TokenClass is TSeparator))
                yield return CreateChild(target.Right, true).Edits;
        }

        ISourcePartEdit GetLineSplitter(Syntax target, bool isInsideChain)
        {
            var second = target.Right;
            if(isInsideChain)
                second = second.Left;

            if(!AdditionalLineBreaksForMultilineItems || second == null)
                return SourcePartEditExtension.MinimalLineBreak;

            if(GetHasAlreadyLineBreakOrIsTooLong(target.Left) || GetHasAlreadyLineBreakOrIsTooLong(second))
                return SourcePartEditExtension.MinimalLineBreaks;

            return SourcePartEditExtension.MinimalLineBreak;
        }

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Target.Token.Characters.Id;
    }
}