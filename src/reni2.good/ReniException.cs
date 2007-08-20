using System;
using System.Collections;
using System.Diagnostics;
using HWClassLibrary.Debug;

namespace Reni
{
	/// <summary>
	/// Summary description for ReniException.
	/// </summary>
	public class ReniException: Exception
	{
	    ArrayList _l = new ArrayList();
		string _text;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="text"></param>
		public ReniException(string text)
		{
            StackTrace st = new StackTrace(true);
			for(int i=1; i<st.FrameCount; i++)
				_l.Add(Tracer.MethodHeader(i,true));
            Tracer.FlaggedLine(text);
            _text = text;
        }
	}                                   
}
