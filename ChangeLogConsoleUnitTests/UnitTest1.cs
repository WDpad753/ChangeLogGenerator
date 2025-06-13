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
            if (logwriter == null)
            {
                Assert.Fail("LogWriter is not initialized.");
            }
            else
            {
                Assert.Pass();
            }
        }
    }
}