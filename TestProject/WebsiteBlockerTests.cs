using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestrictRService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictRService.Tests
{
    [TestClass()]
    public class WebsiteBlockerTests
    {
        private WebsiteBlocker _blocker = null!;

        [TestInitialize()]
        public void TestInitialize()
        {
            _blocker = new WebsiteBlocker();
        }

        [TestCleanup()]
        public void TestCleanup()
        {

        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetBlockedWebsites_NullParameter_ThrowsArgumentNullException()
        {
            ConfigurationPacket.BlockedWebsites? blockedWebsites = null;

            _blocker.SetBlockedWebsites(blockedWebsites);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void SetBlockedWebsites_EmptyBlockedWebsites_ThrowsArgumentException()
        {
            ConfigurationPacket.BlockedWebsites blockedWebsites  = new ConfigurationPacket.BlockedWebsites();

            _blocker.SetBlockedWebsites(blockedWebsites);
        }
    }
}