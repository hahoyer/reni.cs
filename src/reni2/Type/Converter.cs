using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Basics;

namespace Reni.Type
{
    interface IConverter
    {
        TypeBase TargetType { get; }
        Result Result(Category category);
    }

    static class ConverterExtension
    {
        public static IConverter Concat(this IConverter first, Func<Category, Result> second)
        {
            return new ConcatConverter(first, new FunctionalConverter(second));
        }
        public static IConverter Concat(this IConverter first, IConverter second) { return new ConcatConverter(first, second); }
    }

    sealed class ConcatConverter : DumpableObject, IConverter
    {
        readonly IConverter _first;
        readonly IConverter _second;

        public ConcatConverter(IConverter first, IConverter second)
        {
            _first = first;
            _second = second;
        }

        TypeBase IConverter.TargetType { get { return _second.TargetType; } }

        public Result Result(Category category)
        {
            StartMethodDump(false, category);
            try
            {
                var first = _first.Result(category.Typed);
                var second = _second.Result(category);
                Dump("first", first);
                Dump("second", second);
                BreakExecution();
                return ReturnMethodDump(second.ReplaceArg(first));
            }
            finally
            {
                EndMethodDump();
            }
        }
    }

    sealed class FunctionalConverter : IConverter
    {
        readonly Func<Category, Result> _function;

        public FunctionalConverter(Func<Category, Result> function) { _function = function; }
        TypeBase IConverter.TargetType { get { return _function(Category.Type).Type; } }
        public Result Result(Category category) { return _function(category); }
    }
}