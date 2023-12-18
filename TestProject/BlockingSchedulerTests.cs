using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    [TestClass]
    public class BlockingSchedulerTests
    {
        private Mock<IApplicationBlocker> _mockAppBlocker = null!;
        private Mock<IWebsiteBlocker> _mockWebBlocker = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private BlockingScheduler _blockingScheduler = null!;

        [TestInitialize]
        public void TestSetup()
        {
            _mockAppBlocker = new Mock<IApplicationBlocker>();
            _mockWebBlocker = new Mock<IWebsiteBlocker>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            _blockingScheduler = new BlockingScheduler(_mockAppBlocker.Object, _mockWebBlocker.Object, _mockServiceScopeFactory.Object);
        }

        [TestMethod]
        public void CheckSchedule_ShouldImplementBlocking_WhenEventIsActive()
        {
            // Arrange
            Event activeEvent = new()
            {
                // properties to make the event active

                Title = "testing event",
                Start = DateTime.Now.Subtract(TimeSpan.FromHours(1)),
                Duration = TimeSpan.FromHours(4),
                Recurrence = Event.RecurrenceType.None,
                BlockedApps = new()
                {
                    new ApplicationInfo("displayName", "displayVersion", "publisher", "installDate", "installLocation", "comments", "uninstallString", "registryPath")
                },
                BlockedSites = new()
                {
                    BlockAllSites = false,
                    BlockedWebsiteUrls = new()
                    {
                        new BlockedWebsiteUrl()
                        {
                            Url = "example.com"
                        }
                    }
                }
                    
            };

            List<Event> events = new()
            {
                activeEvent
            };

            // Act
            _blockingScheduler.SetConfiguration(events);


            // Assert
            _mockAppBlocker.Verify(appBlocker => appBlocker.SetBlockedApps(It.IsAny<List<string>>()), Times.Once);
            _mockWebBlocker.Verify(webBlocker => webBlocker.SetBlockedWebsites(It.IsAny<BlockedWebsites>()), Times.Once);
        }


        //[TestMethod]
        //public void CheckSchedule_ShouldNotImplementBlocking_WhenEventIsNotActive()
        //{
        //    // Arrange, Act, Assert as in the previous example
        //}

        //[TestMethod]
        //public void CheckSchedule_ShouldTransitionCorrectly_BetweenEvents()
        //{
        //    // Arrange, Act, Assert as in the previous example
        //}



    }
}
