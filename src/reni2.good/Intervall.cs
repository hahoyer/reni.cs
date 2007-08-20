using System;

namespace Reni
{
    /// <summary>
    /// Intervall of 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Intervall<T>: ReniObject
    {
        T _start;
        T _end;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Intervall(T start, T end)
        {
            _start = start;
            _end = end;
        }
        /// <summary>
        /// start
        /// </summary>
        public T Start{get { return _start; }}
        /// <summary>
        /// end
        /// </summary>
        public T End{get { return _end; }}

    }
}
