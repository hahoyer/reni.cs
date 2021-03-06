using System;
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
        [Obsolete("",true)]
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

        [Obsolete("",true)]
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

            [Obsolete("",true)]
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

        [Obsolete("",true)]
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
            [Obsolete("",true)]
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

        static bool GetIsSeparatorRequired(BinaryTree target)
            => !target.Token.PrecededWith.HasComment() &&
               SeparatorExtension.Get(target.LeftNeighbor?.TokenClass, target.TokenClass);

        static bool True => true;
        static bool False => false;
        bool AdditionalLineBreaksForMultilineItems;

        internal Configuration Configuration;
        bool ForceLineSplit;

        bool IsIndentRequired;
        internal BinaryTree Target;

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Obsolete("",true)]
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
                    //Tracer.Assert(CheckMultilineExpectations(result), Binary.Dump);

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
        [Obsolete("",true)]
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

        IEnumerable<ISourcePartEdit> GetWhiteSpacesEdits(BinaryTree target)
        {
            if(target.Token.PrecededWith.Any())
                return T(new WhiteSpaceView(target.Token.PrecededWith, Configuration, GetIsSeparatorRequired(target)));
            return T(new EmptyWhiteSpaceView(target.Token.Characters, GetIsSeparatorRequired(target)));
        }

        [Obsolete("",true)]
        HierarchicalStructure CreateChild(BinaryTree target, bool isLast = false)
        {
            var child = Create(target.TokenClass, isLast);
            child.Configuration = Configuration;
            child.Target = target;
            return child;
        }

        [Obsolete("",true)]
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

        bool GetHasAlreadyLineBreakOrIsTooLong(BinaryTree target)
        {
            var basicLineLength = target.GetFlatLength(Configuration.EmptyLineLimit != 0);
            return basicLineLength == null || basicLineLength > Configuration.MaxLineLength;
        }

        [Obsolete("",true)]
        IEnumerable<IEnumerable<ISourcePartEdit>> GetEditGroupsForChains<TSeparator>()
            where TSeparator : ITokenClass
            => Target
                .Chain(target => target.Right)
                .TakeWhile(target => target.TokenClass is TSeparator)
                .Select(GetEdits<TSeparator>)
                .SelectMany(i => i);

        [Obsolete("",true)]
        IEnumerable<IEnumerable<ISourcePartEdit>> GetEdits<TSeparator>(BinaryTree target)
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

        [Obsolete("",true)]
        ISourcePartEdit GetLineSplitter(BinaryTree target, bool isInsideChain)
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