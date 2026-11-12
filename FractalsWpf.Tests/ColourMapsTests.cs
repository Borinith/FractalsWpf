using FractalsWpf.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FractalsWpf.Tests
{
    public class ColourMapsTests
    {
        [Test]
        public async Task ColourMapNotUsingLambdas()
        {
            var actual = ColourMaps.GetColourMapRgbaValues(ColourMapEnum.Jet, 16);

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

            await AssertRgbaValues(actual, expected);
        }

        [Test]
        public async Task ColourMapUsingLambdas()
        {
            var actual = ColourMaps.GetColourMapRgbaValues(ColourMapEnum.Ocean, 16);

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

            await AssertRgbaValues(actual, expected);
        }

        private static async Task AssertRgbaValues(IReadOnlyList<double[]> actual, IReadOnlyList<double[]> expected)
        {
            const double tolerancePercent = 0.0001;
            await Assert.That(actual.Count).IsEqualTo(expected.Count);

            for (var index = 0; index < actual.Count; index++)
            {
                await Assert.That(actual[index][0]).IsEqualTo(expected[index][0]).WithinRelativeTolerance(tolerancePercent);
                await Assert.That(actual[index][1]).IsEqualTo(expected[index][1]).WithinRelativeTolerance(tolerancePercent);
                await Assert.That(actual[index][2]).IsEqualTo(expected[index][2]).WithinRelativeTolerance(tolerancePercent);
                await Assert.That(actual[index][3]).IsEqualTo(expected[index][3]).WithinRelativeTolerance(tolerancePercent);
            }
        }
    }
}