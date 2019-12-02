using Expressions.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Expressions.Domain.Tests
{
    [TestClass()]
    public class ExpressionTests
    {
        [TestMethod()]
        public void GetResultTest()
        {
            Assert.AreEqual((129 / 3.14f) + 8, new Expression("((1 + 2) * 43) / 3.14 + 2 ^ 3").GetResult());
            Assert.AreEqual(-1, new Expression("-1").GetResult());
            Assert.AreEqual(2 - 2 - 1, new Expression("2*1-2*1-1*1").GetResult());
            Assert.AreEqual(-(2 - 2) - 4 % (1 * 28 - 4 / 4 + (2 * 2 ^ 6)), new Expression("(-(2 - 2) - 4%(1*28-4/4+(2*2^6)))").GetResult());
        }
    }
}