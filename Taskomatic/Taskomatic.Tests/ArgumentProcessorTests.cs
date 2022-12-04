using Taskomatic.Core;
using Xunit;

namespace Taskomatic.Tests
{
    public class ArgumentProcessorTests
    {
        [Fact]
        public void ArgumentProcessorShouldPutSpaceBetweenArguments()
        {
            Assert.Equal(
                "a b c",
                ArgumentProcessor.CygwinArgumentsToString(new[]
                {
                    "a", "b", "c"
                }));
        }

        [Fact]
        public void ArgumentProcessorShouldPutQuotesAroundSpaces()
        {
            Assert.Equal(
                "\"a b\" c",
                ArgumentProcessor.CygwinArgumentsToString(new[]
                {
                    "a b", "c"
                }));
        }

        [Fact]
        public void ArgumentProcessorShouldEscapeQuotes()
        {
            Assert.Equal(
                "\"a \\\" b\" c",
                ArgumentProcessor.CygwinArgumentsToString(new[]
                {
                    "a \" b", "c"
                }));
        }
    }
}
