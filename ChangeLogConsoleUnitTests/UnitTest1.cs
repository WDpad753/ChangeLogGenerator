using BaseLogger;
using Common.Abstractions;

namespace ChangeLogConsoleUnitTests
{
    public class Tests
    {
        private ILogWriter logwriter;

        [SetUp]
        public void Setup()
        {
            logwriter = new LogWriter(@"Config\AppTest.config","");
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}