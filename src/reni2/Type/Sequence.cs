using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    /// <summary>
    /// Special array 
    /// </summary>
    internal sealed class Sequence : Base
    {
        [Node]
        public Array InheritedType { get { return _inheritedType; } }
        private readonly Array _inheritedType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sequence"/> class.
        /// </summary>
        /// <param name="elementType">Type of the element.</param>
        /// <param name="count">The count.</param>
        /// created 13.01.2007 14:59
        public Sequence(Base elementType, int count)
        {
            Tracer.Assert(count > 0); 
            _inheritedType = elementType.CreateArray(count);
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return _inheritedType.Size; } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        internal override string DumpPrintText
        {
            get
            {
                return "(" + _inheritedType.Element.DumpPrintText + ")sequence(" + _inheritedType.Count + ")";
            }
        }
        /// <summary>
        /// Default dump behaviour
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 21:18 on HAHOYER-DELL by hh
        public override string Dump()
        {
            return InheritedType.Dump();
        }

        /// <summary>
        /// Searches the definable defineableToken at type
        /// </summary>
        /// <param name="defineableToken">The token.</param>
        /// <returns></returns>
        internal override SearchResult Search(DefineableToken defineableToken)
        {
            return defineableToken.TokenClass.Search(this);
        }

        /// <summary>
        /// Searches the definable token at type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal override PrefixSearchResult PrefixSearchDefineable(DefineableToken t)
        {
            PrefixSearchResult result = Element.PrefixSearchDefineableFromSequence(t,Count);
            if (result != null)
                return result;
            return base.PrefixSearchDefineable(t);
        }

        /// <summary>
        /// Gets the type of the sequence element.
        /// </summary>
        /// <value>The type of the sequence element.</value>
        /// created 13.01.2007 19:46
        internal override Base SequenceElementType { get { return Element; } }

        /// <summary>
        /// Gets the type of the sequence element.
        /// </summary>
        /// <value>The type of the sequence element.</value>
        /// created 13.01.2007 19:46
        internal override int SequenceCount { get { return Count; } }

        /// <summary>
        /// Determines whether [is convertable to] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="conversionFeature">The conversion feature.</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        /// created 11.01.2007 22:09
        internal override bool IsConvertableToVirt(Base dest, ConversionFeature conversionFeature)
        {
            Pending destPending = dest as Pending;
            if(destPending != null)
                return true;

            Sequence destSequence = dest as Sequence;
            if(destSequence != null)
            {
                if(conversionFeature.IsDisableCut && Count > destSequence.Count)
                    return false;
                return Element.IsConvertableTo(destSequence.Element, conversionFeature.DontUseConverter);
            }

            Aligner destAligner = dest as Aligner;
            if (destAligner != null)
                return IsConvertableTo(destAligner.Parent, conversionFeature);

            return base.IsConvertableToVirt(dest, conversionFeature);
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        internal override Result ConvertToVirt(Category category, Base dest)
        {
            Pending destPending = dest as Pending;
            if (destPending != null)
                return Result.CreatePending(category);

            Result result = ConvertTo(category, dest as Sequence);
            if(result != null) return result;

            result = ConvertTo(category, dest as Aligner);
            if (result != null) return result;

            NotImplementedMethod(category, dest);
            return null;

        }

        private Result ConvertTo(Category category, Sequence dest)
        {
            if(dest == null)return null;

            Result result = CreateArgResult(category);
            if(Count > dest.Count)
                result = RemoveElementsAtEnd(category, dest.Count);

            if(Element != dest.Element)
            {
                Result elementResult = Element.ConvertTo(category, dest.Element);
                NotImplementedMethod(category, dest, "result", result, "elementResult", elementResult);
                return null;
            }
            if(Count < dest.Count)
                result = dest.ExtendFrom(category, Count).UseWithArg(result);
            return result;
        }

        private Result ConvertTo(Category category, Aligner dest)
        {
            if (dest == null) return null;
            return ConvertTo(category, dest.Parent).Align(dest.AlignBits);
        }

        /// <summary>
        /// Adds the elements at end.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="oldCount">The old count.</param>
        /// <returns></returns>
        /// created 15.01.2007 02:25
        private Result ExtendFrom(Category category, int oldCount)
        {
            Size oldSize = Element.Size * oldCount;
            Result result = CreateResult
                (
                category,
                delegate{return Code.Base.CreateArg(oldSize).CreateBitCast(Size);}
                );
            return result;
        }

        /// <summary>
        /// Removes the elements at end.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="newCount">The new count.</param>
        /// <returns></returns>
        /// created 14.01.2007 16:11
        private Result RemoveElementsAtEnd(Category category, int newCount)
        {
            Result destructor = Element.DestructorHandler(category);
            if(!destructor.IsEmpty)
            {
                NotImplementedMethod(category,newCount,"destructor",destructor);
                return null;
            }
            int tempNewCount = Math.Min(Count, newCount);
            Sequence newType = Element.CreateSequence(tempNewCount);
            Result result = newType
                .CreateResult
                (
                category,
                delegate { return Code.Base.CreateArg(Size).CreateBitCast(newType.Size); }
                );
            return result;
        }

        internal int Count { get { return _inheritedType.Count; } }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>The element.</value>
        /// created 13.01.2007 19:34
        public Base Element { get { return _inheritedType.Element; } }
        /// <summary>
        /// Dumps the print code.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        internal override Result DumpPrint(Category category)
        {
            return Element.SequenceDumpPrint(category, Count);
        }

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal override Result DestructorHandler(Category category)
        {
            return _inheritedType.DestructorHandler(category);
        }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        internal override Result MoveHandler(Category category)
        {
            return _inheritedType.MoveHandler(category);
        }

    }

    internal sealed class SequenceOperationResult : SearchResult
    {
        [DumpData(true)]
        private readonly Defineable _defineable;
        private readonly Sequence _sequence;

        public SequenceOperationResult(Sequence sequence, Defineable defineable)
            : base(sequence)
        {
            _sequence = sequence;
            _defineable = defineable;
        }

        ///// <summary>
        ///// Creates the result for member function searched. Object is provided by use of "Arg" code element
        ///// </summary>
        ///// <param name="callContext">The call context.</param>
        ///// <param name="category">The category.</param>
        ///// <param name="args">The args.</param>
        ///// <returns></returns>
        //public override Result VisitApplyFromRef(ContextAtPosition.Base callContext, Category category, Syntax.Base args)
        //{
        //    return _defineable.VisitSequenceOperationApply(callContext, category, args, _sequence);
        //}
        /// <summary>
        /// Creates the result for member function searched. Object is provided by use of "Arg" code element
        /// </summary>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
        {
            NotImplementedMethod(callContext, category, args);
            return null;
        }
    }
}