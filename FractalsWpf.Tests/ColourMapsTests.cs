using System.Collections.Generic;
using Xunit;

namespace FractalsWpf.Tests
{
    public class ColourMapsTests
    {
        [Fact]
        public void ColourMapNotUsingLambdas()
        {
            var actual = ColourMaps.GetColourMapRgbaValues("jet", 16);

            var expected = new[]
            {
                new[] { 0d, 0, 0.5, 1 },
                new[] { 0, 0, 0.8030303, 1 },
                new[] { 0, 0.03333333, 1, 1 },
                new[] { 0, 0.3, 1, 1 },
                new[] { 0, 0.56666667, 1, 1 },
                new[] { 0, 0.83333333, 1, 1 },
                new[] { 0.16129032, 1, 0.80645161, 1 },
                new[] { 0.37634409, 1, 0.59139785, 1 },
                new[] { 0.59139785, 1, 0.37634409, 1 },
                new[] { 0.80645161, 1, 0.16129032, 1 },
                new[] { 1, 0.90123457, 0, 1 },
                new[] { 1, 0.65432099, 0, 1 },
                new[] { 1, 0.40740741, 0, 1 },
                new[] { 1, 0.16049383, 0, 1 },
                new[] { 0.8030303, 0, 0, 1 },
                new[] { 0.5, 0, 0, 1 }
            };

            AssertRgbaValues(actual, expected);
        }

        [Fact]
        public void ColourMapUsingLambdas()
        {
            var actual = ColourMaps.GetColourMapRgbaValues("ocean", 16);

            var expected = new[]
            {
                new[] { 0, 0.5, 0, 1 },
                new[] { 0, 0.4, 0.06666667, 1 },
                new[] { 0, 0.3, 0.13333333, 1 },
                new[] { 0, 0.2, 0.2, 1 },
                new[] { 0, 0.1, 0.26666667, 1 },
                new[] { 0, 0, 0.33333333, 1 },
                new[] { 0, 0.1, 0.4, 1 },
                new[] { 0, 0.2, 0.46666667, 1 },
                new[] { 0, 0.3, 0.53333333, 1 },
                new[] { 0, 0.4, 0.6, 1 },
                new[] { 0, 0.5, 0.66666667, 1 },
                new[] { 0.2, 0.6, 0.73333333, 1 },
                new[] { 0.4, 0.7, 0.8, 1 },
                new[] { 0.6, 0.8, 0.86666667, 1 },
                new[] { 0.8, 0.9, 0.93333333, 1 },
                new[] { 1d, 1, 1, 1 }
            };

            AssertRgbaValues(actual, expected);
        }

        private static void AssertRgbaValues(IReadOnlyList<double[]> actual, IReadOnlyList<double[]> expected)
        {
            const double tolerancePercent = 0.0001;
            Assert.Equal(actual.Count, expected.Count);

            for (var index = 0; index < actual.Count; index++)
            {
                Assert.Equal(actual[index][0], expected[index][0], expected[index][0] * tolerancePercent);
                Assert.Equal(actual[index][1], expected[index][1], expected[index][1] * tolerancePercent);
                Assert.Equal(actual[index][2], expected[index][2], expected[index][2] * tolerancePercent);
                Assert.Equal(actual[index][3], expected[index][3], expected[index][3] * tolerancePercent);
            }
        }
    }
}