using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
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
                    NotImplementedMethod();
                    return default;
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
            new readonly Reni.SyntaxTree.Syntax Target;

            public ExpressionFrame(Reni.SyntaxTree.Syntax target) => Target = target;

            protected override IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
            {
                get
                {
                    NotImplementedMethod();
                    return default;
                }
            }
        }

        class ColonFrame : HierarchicalStructure
        {
            [DisableDump]
            protected override IEnumerable<IEnumerable<ISourcePartEdit>> EditGroups
                => GetEditGroupsForChains<Colon>();

            protected override HierarchicalStructure Create(Reni.SyntaxTree.Syntax tokenClass, bool isLast)
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
                    NotImplementedMethod();
                    return default;
                }
            }
        }

        internal Configuration Configuration;
        internal Syntax Target;
        bool AdditionalLineBreaksForMultilineItems;
        bool ForceLineSplit;

        bool IsIndentRequired;

        static bool True => true;
        static bool False => false;

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
                    //Tracer.Assert(CheckMultilineExpectations(result), Anchor.Dump);

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
        IndentDirection IndentDirection => IsIndentRequired? IndentDirection.ToRight : IndentDirection.NoIndent;

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

        bool GetIsSeparatorRequired(BinaryTree target)
            => !target.Token.PrecededWith.HasComment() &&
               SeparatorExtension.Get(Target.GetLeftNeighbor(target)?.TokenClass, target.TokenClass);

        //bool CheckMultilineExpectations(IEnumerable<ISourcePartEdit> result)
        //    => Target.Left == null && Target.Right == null && IsLineSplit ||
        //       result.Skip(1).Any(edit => edit.HasLines) == IsLineSplit;

        IEnumerable<ISourcePartEdit> GetWhiteSpacesEdits(BinaryTree target)
        {
            if(target.Token.PrecededWith.Any())
                return T(new WhiteSpaceView(target.Token.PrecededWith, Configuration, GetIsSeparatorRequired(target)));
            return T(new EmptyWhiteSpaceView(target.Token.Characters.Start, GetIsSeparatorRequired(target)));
        }

        HierarchicalStructure CreateChild(Syntax target, bool isLast = false)
        {
            var child = Create(target.FlatItem, isLast);
            child.Configuration = Configuration;
            child.Target = target;
            return child;
        }

        protected virtual HierarchicalStructure Create(Reni.SyntaxTree.Syntax target, bool isLast)
        {
            switch(target)
            {
                case CompoundSyntax _:
                    return new ListFrame();
                case DeclarationSyntax _:
                    return new ColonFrame();
                default:
                    return new ExpressionFrame(target);
            }
        }

        bool GetHasAlreadyLineBreakOrIsTooLong(Syntax target)
        {
            var basicLineLength = target.GetFlatLength(Configuration.EmptyLineLimit != 0);
            return basicLineLength == null || basicLineLength > Configuration.MaxLineLength;
        }

        IEnumerable<IEnumerable<ISourcePartEdit>> GetEditGroupsForChains<TSeparator>()
            where TSeparator : ITokenClass
        {
            NotImplementedMethod();
            return default;
        }

        //IEnumerable<IEnumerable<ISourcePartEdit>> GetEdits<TSeparator>(BinaryTree target)
        //    where TSeparator : ITokenClass
        //{
        //    yield return CreateChild(target.Left).Edits;
        //    yield return GetWhiteSpacesEdits(target);

        //    if(target.Right == null)
        //        yield break;

        //    if(IsLineSplit)
        //        yield return T(GetLineSplitter(target, target.Right?.TokenClass is TSeparator));

        //    if(!(target.Right.TokenClass is TSeparator))
        //        yield return CreateChild(target.Right, true).Edits;
        //}

        //ISourcePartEdit GetLineSplitter(BinaryTree target, bool isInsideChain)
        //{
        //    var second = target.Right;
        //    if(isInsideChain)
        //        second = second.Left;

        //    if(!AdditionalLineBreaksForMultilineItems || second == null)
        //        return SourcePartEditExtension.MinimalLineBreak;

        //    if(GetHasAlreadyLineBreakOrIsTooLong(target.Left) || GetHasAlreadyLineBreakOrIsTooLong(second))
        //        return SourcePartEditExtension.MinimalLineBreaks;

        //    return SourcePartEditExtension.MinimalLineBreak;
        //}
    }
}