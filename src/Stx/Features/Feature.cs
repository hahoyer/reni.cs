using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Stx.Features
{
    sealed class Feature : DumpableObject
    {
        sealed class SimpleFeature : EnumEx
        {
            public static readonly SimpleFeature CodeItems = new SimpleFeature();
            public static readonly SimpleFeature DataType = new SimpleFeature();
            SimpleFeature() {}
        }

        static readonly List<Feature> WellKnown = new List<Feature>();

        static int NextObjectId;

        static Feature Convert(params SimpleFeature[] features) => Convert1(features);

        static Feature Convert1(IEnumerable<SimpleFeature> features)
        {
            var f = features.OrderBy(i => i.GetType().Name).Distinct().ToArray();
            var result = WellKnown.FirstOrDefault(i => i.IsMatch(f));
            if(result != null)
                return result;
            
            result = new Feature(f);
            WellKnown.Add(result);

            return result;
        }

        public static Feature None => Convert();
        public static Feature DataType => Convert(SimpleFeature.DataType);
        public static Feature CodeItems => Convert(SimpleFeature.CodeItems);

        public static Feature operator+(Feature left, Feature right) => left.Combine(right);
        public static Feature operator-(Feature left, Feature right) => left.Without(right);
        public static bool operator<=(Feature left, Feature right) => left.Contains(right);
        public static bool operator>=(Feature left, Feature right) => right.Contains(left);

        [EnableDump]
        readonly SimpleFeature[] Data;

        Feature(SimpleFeature[] data)
            : base(NextObjectId++)
        {
            Data = data;
        }

        bool IsMatch(IReadOnlyList<SimpleFeature> features)
            => Data.Length == features.Count && !Data.Where((d, i) => d != features[i]).Any();

        Feature Without(Feature right)
            => Convert1(Data.Where(data => !right.Data.Contains(data)));

        Feature Combine(Feature right)
            => Convert1(Data.Concat(right.Data));

        bool Contains(Feature right)
            => right.Data.All(data => Data.Contains(data));
    }
}