using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Parser;

namespace Reni.Type
{
	/// <summary>
	/// Summary description for CreateRef.
	/// </summary>
	public class Ref: Child
	{
	    static private int _nextObjectId = 0; 
	    private readonly RefAlignParam _refAlignParam;

	    /// <summary>
        /// ctor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="refAlignParam"></param>
        public Ref(Base target, RefAlignParam refAlignParam) : base(_nextObjectId++, target)
	    {
            _refAlignParam = refAlignParam;
        }
        /// <summary>
        /// asis
        /// </summary>
        [Node, DumpData(false)]
        public RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        /// <summary>
        /// Target of reference
        /// </summary>
        [DumpData(false)]
        public Base Target { get { return Parent; } }
        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return RefAlignParam.RefSize; } }

	    /// <summary>
	    /// Gets a value indicating whether this instance is ref.
	    /// </summary>
	    /// <value><c>true</c> if this instance is ref; otherwise, <c>false</c>.</value>
	    /// [created 01.06.2006 22:51]
        [DumpData(false)]
        public override bool IsRef { get { return true; } }

	    /// <summary>
	    /// Gets the size of the unref.
	    /// </summary>
	    /// <value>The size of the unref.</value>
	    /// [created 06.06.2006 00:08]
	    public override Size UnrefSize { get { return Target.Size; } }

	    /// <summary>
	    /// Create a reference to a type
	    /// </summary>
	    /// <param name="refAlignParam">Alignment  and size of the reference</param>
	    /// <returns></returns>
	    public override Ref CreateRef(RefAlignParam refAlignParam)
	    {
	        if(refAlignParam.IsEqual(RefAlignParam) )
	            return this;
	        return base.CreateRef(refAlignParam);
	    }

	    /// <summary>
	    /// Gets the dump print text.
	    /// </summary>
	    /// <value>The dump print text.</value>
	    /// created 08.01.2007 17:54
        [DumpData(false)]
        public override string DumpPrintText { get { return "#(#ref#)# " + Parent.DumpPrintText; } }

        /// <summary>
        /// Searches the definable token at type
        /// </summary>
        /// <param name="token">The t.</param>
        /// <returns></returns>
        public override SearchResult SearchDefineable(DefineableToken token)
	    {
	        SearchResult result = token.TokenClass.RefOperation(this);
            if (result != null)
                return result;
            result = Target.SearchDefineable(token);
	        if(result!=null)
                return result.FoundFromRef(RefAlignParam);
	        return base.SearchDefineable(token);
	    }

	    /// <summary>
	    /// Gets the type of the sequence element.
	    /// </summary>
	    /// <value>The type of the sequence element.</value>
	    /// created 13.01.2007 19:46
	    internal override int SequenceCount { get { return Target.SequenceCount; } }

	    /// <summary>
	    /// Destructors the specified category.
	    /// </summary>
	    /// <param name="category">The category.</param>
	    /// <returns></returns>
	    /// [created 02.06.2006 09:47]
	    public override Result DestructorHandler(Category category)
	    {
	        return EmptyHandler(category);
	    }

	    /// <summary>
	    /// Moves the handler.
	    /// </summary>
	    /// <param name="category">The category.</param>
	    /// <returns></returns>
	    /// [created 05.06.2006 16:47]
	    public override Result MoveHandler(Category category)
	    {
            return EmptyHandler(category);
        }

        /// <summary>
        /// Checks if type is a reference and dereferences instance.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// created 05.01.2007 01:10
        public override Result Dereference(Result result)
        {
            return CreateDereferencedArgResult(result.Complete).UseWithArg(result);
        }

	    /// <summary>
	    /// Dumps the print code.
	    /// </summary>
	    /// <param name="category">The category.</param>
	    /// <returns></returns>
	    /// created 08.01.2007 17:29
	    internal override Result DumpPrint(Category category)
	    {
            return Target
                .DumpPrint(category)
                .UseWithArg(CreateDereferencedArgResult(category));
	    }

	    /// <summary>
        /// Applies the type operator.
        /// </summary>
        /// <param name="argResult">The arg result.</param>
        /// <returns></returns>
        /// created 10.01.2007 15:45
	    public override Result ApplyTypeOperator(Result argResult)
	    {
            return Parent.ApplyTypeOperator(argResult);
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
	        return Target
                .ConvertTo(category, dest)
                .UseWithArg(CreateDereferencedArgResult(category));
        }

	    private Result CreateDereferencedArgResult(Category category)
	    {
            return Target.CreateResult
                (
                category, 
                delegate { return CreateDereferencedArgCode(); }
                );
        }

        /// <summary>
        /// Creates the dereferenced arg code.
        /// </summary>
        /// <returns></returns>
        /// created 01.02.2007 00:04
	    public Code.Base CreateDereferencedArgCode()
	    {
	        return Code.Base.CreateArg(Size).CreateDereference(RefAlignParam,Target.Size);
	    }

	    /// <summary>
	    /// Determines whether [is convertable to] [the specified dest].
	    /// </summary>
	    /// <param name="dest">The dest.</param>
	    /// <param name="useConverter">if set to <c>true</c> [use converter].</param>
	    /// <returns>
	    /// 	<c>true</c> if [is convertable to] [the specified dest]; otherwise, <c>false</c>.
	    /// </returns>
	    /// created 11.01.2007 22:09
	    internal override bool IsConvertableToVirt(Base dest, bool useConverter)
	    {
	        return Target.IsConvertableTo(dest, useConverter);
	    }

        /// <summary>
        /// Assignements the operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// created 16.02.2007 22:59
	    public Result AssignementOperator(Result value)
	    {
            Result convertedValue = value.ConvertTo(Target);
            Category category = value.Complete;
            Result result = CreateVoid.CreateResult
                (
                category,
                delegate { return Code.Base.CreateArg(Size).CreateAssign(RefAlignParam, convertedValue.Code); },
                delegate { return convertedValue.Refs; }
                );
            if (Target.DestructorHandler(category).IsEmpty && Target.MoveHandler(category).IsEmpty)
                return result;

            NotImplementedMethod(value, "result", result);
	        throw new NotImplementedException();
	    }
	}
}

