//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TestProject
//{
//    [TestClass()]
//    public class ConfigurationPacketTests
//    {
//        [TestMethod()]
//        [ExpectedException(typeof(InvalidOperationException))]
//        public void ConfigPacket_SetsAllAndEmptyList_InvalidOperationException()
//        {
//            Event.BlockedWebsites blockedWebsites = new()
//            {
//                BlockAllSites = true,
//                BlockedWebsiteUrls = new List<string>()
//            };
//        }

//        [TestMethod()]
//        [ExpectedException(typeof(InvalidOperationException))]
//        public void ConfigPacket_SetsAllAndFilledList_InvalidOperationException()
//        {
//            Event.BlockedWebsites blockedWebsites = new()
//            {
//                BlockAllSites = true,
//                BlockedWebsiteUrls = new List<string>
//                {
//                    "example.com"
//                }
//            };
//        }

//        [TestMethod()]
//        [ExpectedException(typeof(InvalidOperationException))]
//        public void ConfigPacket_SetsEmptyListAndAll_InvalidOperationException()
//        {
//            Event.BlockedWebsites blockedWebsites = new()
//            {
//                BlockedWebsiteUrls = new List<string>(),
//                BlockAllSites = true,
//            };
//        }

//        [TestMethod()]
//        [ExpectedException(typeof(InvalidOperationException))]
//        public void ConfigPacket_SetsFilledListAndAll_InvalidOperationException()
//        {
//            Event.BlockedWebsites blockedWebsites = new()
//            {
//                BlockedWebsiteUrls = new List<string>
//                {
//                    "example.com"
//                },
//                BlockAllSites = true
//            };
//        }
//    }
//}
