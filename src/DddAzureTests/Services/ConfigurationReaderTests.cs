using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ddd.Services.Tests
{
    [TestClass()]
    public class ConfigurationReaderTests
    {
        [TestMethod()]
        public void CanReadSettingsFromAppConfigFile()
        {
            var config = ConfigurationReader<TestConfig>.GetConfig();
            Assert.AreEqual("Value1", config.TestProperty1);
            Assert.AreEqual("Value2", config.TestProperty2);
        }

        internal class TestConfig
        {
            public string TestProperty1 { get; set; }
            public string TestProperty2 { get; set; }
        }
    }
}