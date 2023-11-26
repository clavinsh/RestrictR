using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestrictRService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetFwTypeLib; // For managing Windows Firewall rules
using System.Xml.Linq;

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
            _blocker.ClearBlockedWebsites();
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            _blocker.ClearBlockedWebsites();
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetBlockedWebsites_NullParameter_ThrowsArgumentNullException()
        {
            ConfigurationPacket.BlockedWebsites? blockedWebsites = null;

            _blocker.SetBlockedWebsites(blockedWebsites);
        }

        [TestMethod()]
        public void SetBlockedWebsites_Empty_NoFWRulesCreated()
        {
            ConfigurationPacket.BlockedWebsites blockedWebsites = new();

            _blocker.SetBlockedWebsites(blockedWebsites);

            var rules = _blocker.GetCreatedFwRules();

            Assert.AreEqual(0, rules.Count(), "Unexpected number of rules created.");
        }

        [TestMethod()]
        public void SetBlockedWebsites_AllOfInternet_CreatedFWRule()
        {
            ConfigurationPacket.BlockedWebsites blockedWebsites = new()
            {
                BlockAllSites = true
            };

            _blocker.SetBlockedWebsites(blockedWebsites);

            var rules = _blocker.GetCreatedFwRules();

            Assert.AreEqual(1, rules.Count(), "Unexpected number of rules created.");

            var singleRule = rules.FirstOrDefault();

            Assert.IsNotNull(singleRule, "No rule found.");

            Assert.AreEqual(NET_FW_ACTION_.NET_FW_ACTION_BLOCK, singleRule.Action);
            Assert.AreEqual("Used to block all internet access.", singleRule.Description);
            Assert.AreEqual(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT, singleRule.Direction);
            Assert.AreEqual(true, singleRule.Enabled);
            Assert.AreEqual("All", singleRule.InterfaceTypes);
            Assert.AreEqual("Block Internet", singleRule.Name);
            Assert.AreEqual("RestrictR", singleRule.Grouping);
        }

        [TestMethod()]
        public void SetBlockedWebsites_ListWithExistingHostNames_CreatedFWRules()
        {
            ConfigurationPacket.BlockedWebsites blockedWebsites = new()
            {
                BlockedWebsiteUrls = new List<string>
                {
                    "www.example.com",
                    "www.google.com",
                    "www.microsoft.com",
                    "www.github.com",
                    "www.wikipedia.org",
                    "www.facebook.com",
                    "www.twitter.com",
                    "www.linkedin.com",
                    "www.youtube.com",
                    "www.reddit.com",
                    "www.amazon.com"
                }
            };

            _blocker.SetBlockedWebsites(blockedWebsites);

            var rules = _blocker.GetCreatedFwRules();

            Assert.AreEqual(blockedWebsites.BlockedWebsiteUrls.Count, rules.Count(), "Unexpected number of rules created.");

            CollectionAssert.AreEquivalent(blockedWebsites.BlockedWebsiteUrls, rules.Select(rule => rule.Name).ToList());

            foreach(var rule in rules )
            {
                Assert.AreEqual(NET_FW_ACTION_.NET_FW_ACTION_BLOCK, rule.Action);
                Assert.AreEqual("Used to block internet access to a specific URL.", rule.Description);
                Assert.AreEqual(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT, rule.Direction);
                Assert.AreEqual(true, rule.Enabled);
                Assert.AreEqual("All", rule.InterfaceTypes);
                Assert.AreEqual("RestrictR", rule.Grouping);
            }
        }

        [TestMethod()]
        public void SetBlockedWebsites_ListWithExistingHostNamesNoWWW_CreatedFWRules()
        {
            ConfigurationPacket.BlockedWebsites blockedWebsites = new()
            {
                BlockedWebsiteUrls = new List<string>
                {
                    "example.com",
                    "google.com",
                    "microsoft.com",
                    "github.com",
                    "wikipedia.org",
                    "facebook.com",
                    "twitter.com",
                    "linkedin.com",
                    "youtube.com",
                    "reddit.com",
                    "amazon.com"
                }
            };

            _blocker.SetBlockedWebsites(blockedWebsites);

            var rules = _blocker.GetCreatedFwRules();

            Assert.AreEqual(blockedWebsites.BlockedWebsiteUrls.Count, rules.Count(), "Unexpected number of rules created.");

            CollectionAssert.AreEquivalent(blockedWebsites.BlockedWebsiteUrls, rules.Select(rule => rule.Name).ToList());

            foreach (var rule in rules)
            {
                Assert.AreEqual(NET_FW_ACTION_.NET_FW_ACTION_BLOCK, rule.Action);
                Assert.AreEqual("Used to block internet access to a specific URL.", rule.Description);
                Assert.AreEqual(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT, rule.Direction);
                Assert.AreEqual(true, rule.Enabled);
                Assert.AreEqual("All", rule.InterfaceTypes);
                Assert.AreEqual("RestrictR", rule.Grouping);
            }
        }

        [TestMethod()]
        public void SetBlockedWebsites_ListWithExistingHostNamesFullySpecifiedURI_CreatedFWRules()
        {
            ConfigurationPacket.BlockedWebsites blockedWebsites = new()
            {
                BlockedWebsiteUrls = new List<string>
                {
                    "https://www.google.com",
                    "https://www.microsoft.com",
                    "https://www.github.com",
                    "https://www.wikipedia.org",
                    "https://www.facebook.com",
                    "https://www.twitter.com",
                    "https://www.linkedin.com",
                    "https://www.youtube.com",
                    "https://www.reddit.com",
                    "https://www.amazon.com"
                }
            };

            _blocker.SetBlockedWebsites(blockedWebsites);

            var rules = _blocker.GetCreatedFwRules();

            Assert.AreEqual(blockedWebsites.BlockedWebsiteUrls.Count, rules.Count(), "Unexpected number of rules created.");

            CollectionAssert.AreEquivalent(blockedWebsites.BlockedWebsiteUrls, rules.Select(rule => rule.Name).ToList());

            foreach (var rule in rules)
            {
                Assert.AreEqual(NET_FW_ACTION_.NET_FW_ACTION_BLOCK, rule.Action);
                Assert.AreEqual("Used to block internet access to a specific URL.", rule.Description);
                Assert.AreEqual(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT, rule.Direction);
                Assert.AreEqual(true, rule.Enabled);
                Assert.AreEqual("All", rule.InterfaceTypes);
                Assert.AreEqual("RestrictR", rule.Grouping);
            }
        }

        [TestMethod()]
        public void SetBlockedWebsites_ListWithInvalidHostNames_NoFWRulesCreated()
        {
            ConfigurationPacket.BlockedWebsites blockedWebsites = new()
            {
                BlockedWebsiteUrls = new List<string>
                {
                    "invalid",
                    "example",
                    ":::",
                    "fafsafghjj",
                    "chchcrkc"
                }
            };

            _blocker.SetBlockedWebsites(blockedWebsites);

            var rules = _blocker.GetCreatedFwRules();

            Assert.AreEqual(0, rules.Count(), "Unexpected number of rules created.");
        }

        [TestMethod()]
        public void SetBlockedWebsites_ListWithNonexistantHostNames_NoFWRulesCreated()
        {
            ConfigurationPacket.BlockedWebsites blockedWebsites = new()
            {
                BlockedWebsiteUrls = new List<string>
                {
                    "this.site.does.not.exist.com",
                    "hl.l.lass.fgg.com",
                    "aasadadsaddd.com"
                }
            };

            _blocker.SetBlockedWebsites(blockedWebsites);

            var rules = _blocker.GetCreatedFwRules();

            Assert.AreEqual(0, rules.Count(), "Unexpected number of rules created.");
        }


    }
}
