using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace TestProject
{
    [TestClass]
    public class BlockingSchedulerTests
    {
        private Mock<IApplicationBlocker> _mockAppBlocker = null!;
        private Mock<IWebsiteBlocker> _mockWebBlocker = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IClock> _mockClock = null!;
        private BlockingScheduler _blockingScheduler = null!;

        [TestInitialize]
        public void TestSetup()
        {
            _mockAppBlocker = new Mock<IApplicationBlocker>();
            _mockWebBlocker = new Mock<IWebsiteBlocker>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockClock = new Mock<IClock>();


            // sets up what 'now' means for edge-case testing when datetime.now is not sufficient
            // by default DateTime.Now is used
            _mockClock.Setup(x => x.Now).Returns(DateTime.Now);

            _blockingScheduler = new BlockingScheduler(_mockAppBlocker.Object, _mockWebBlocker.Object, _mockServiceScopeFactory.Object, _mockClock.Object);
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
            _mockAppBlocker.Verify(appBlocker => appBlocker.SetBlockedApps(It.Is<List<string>>(apps => apps.Contains("installLocation"))), Times.Once);
            _mockWebBlocker.Verify(webBlocker => webBlocker.SetBlockedWebsites(It.Is<BlockedWebsites>(site => site.BlockedWebsiteUrls.Any(url => url.Url == "example.com"))), Times.Once);
        }


        [TestMethod]
        public void CheckSchedule_ShouldNotImplementBlocking_WhenEventIsNotActive()
        {
            // Arrange
            Event inactiveEvent = new()
            {
                // properties to make the event inactive

                Title = "testing event",
                Start = DateTime.Now.Add(TimeSpan.FromHours(1)),
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
                inactiveEvent
            };

            // Act
            _blockingScheduler.SetConfiguration(events);


            // Assert
            _mockAppBlocker.Verify(appBlocker => appBlocker.SetBlockedApps(
                It.Is<List<string>>(apps => apps.Contains("installLocation"))),
                Times.Never);

            _mockWebBlocker.Verify(webBlocker => webBlocker.SetBlockedWebsites(
                It.Is<BlockedWebsites>(site => site.BlockedWebsiteUrls.Any(url => url.Url == "example.com"))),
                Times.Never);
        }

        [TestMethod]
        public void CheckSchedule_ShouldImplementBlocking_WhenEventJustStarted()
        {
            // Arrange
            var justStartedEvent = new Event
            {
                // Event that has just started
                Title = "testing event",
                Start = DateTime.Now,
                Duration = TimeSpan.FromHours(1),
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

            var events = new List<Event> { justStartedEvent };

            // Act
            _blockingScheduler.SetConfiguration(events);

            // Assert
            _mockAppBlocker.Verify(appBlocker => appBlocker.SetBlockedApps(
                It.Is<List<string>>(apps => apps.Contains("installLocation"))),
                Times.Never);

            _mockWebBlocker.Verify(webBlocker => webBlocker.SetBlockedWebsites(
                It.Is<BlockedWebsites>(site => site.BlockedWebsiteUrls.Any(url => url.Url == "example.com"))),
                Times.Never);
        }

        [TestMethod]
        public void CheckSchedule_ShouldImplementBlocking_WhenEventIsRecurringAndActive()
        {
            // Arrange
            var recurringEvent = new Event
            {
                Title = "testing event",
                Start = DateTime.Now.AddDays(-7).AddHours(-1), // Exactly one week and one hour ago
                Duration = TimeSpan.FromHours(2),
                Recurrence = Event.RecurrenceType.Weekly,
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

            var events = new List<Event> { recurringEvent };

            // Act
            _blockingScheduler.SetConfiguration(events);

            // Assert

            // The event should be active if it's a weekly recurring event

            _mockAppBlocker.Verify(appBlocker => appBlocker.SetBlockedApps(
                It.Is<List<string>>(apps => apps.Contains("installLocation"))),
                Times.Once);

            _mockWebBlocker.Verify(webBlocker => webBlocker.SetBlockedWebsites(
                It.Is<BlockedWebsites>(site => site.BlockedWebsiteUrls.Any(url => url.Url == "example.com"))),
                Times.Once);
        }

        [TestMethod]
        public void CheckSchedule_ShouldTransitionCorrectly_FromOneEventToAnother()
        {
            // Arrange
            var firstEventStart = DateTime.Today.AddHours(10); // 10 AM today
            var secondEventStart = DateTime.Today.AddHours(12); // 12 AM today

            var firstEvent = new Event
            {
                Title = "testing event",
                Start = firstEventStart,
                Duration = TimeSpan.FromHours(1),
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

            var secondEvent = new Event
            {
                Title = "testing event",
                Start = secondEventStart,
                Duration = TimeSpan.FromHours(1),
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

            var events = new List<Event> { firstEvent, secondEvent };

            // Act

            // Simulate first event is active
            _mockClock.Setup(tp => tp.Now).Returns(firstEventStart.AddMinutes(30)); // During the first event

            _blockingScheduler.SetConfiguration(events);

            // Simulate time moving to the start of the second event
            _mockClock.Setup(tp => tp.Now).Returns(secondEventStart);

            _blockingScheduler.CheckSchedule();

            // Assert
            // Verify transitions between events
            // Assuming that the blocker's Set method is called each time an event becomes active
            _mockAppBlocker.Verify(appBlocker => appBlocker.SetBlockedApps(It.IsAny<List<string>>()), Times.Exactly(2));
            _mockWebBlocker.Verify(webBlocker => webBlocker.SetBlockedWebsites(It.IsAny<BlockedWebsites>()), Times.Exactly(2));
        }

        [TestMethod]
        public void CheckSchedule_NoBlockerCallsWithMulitpleChecks_WhenEventIsActive()
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

            // even with multiple schedule checks there should not be any new blocker function calls
            // since the event has not changed
            _blockingScheduler.CheckSchedule();
            _blockingScheduler.CheckSchedule();
            _blockingScheduler.CheckSchedule();
            _blockingScheduler.CheckSchedule();

            // Assert
            _mockAppBlocker.Verify(appBlocker => appBlocker.SetBlockedApps(It.Is<List<string>>(apps => apps.Contains("installLocation"))), Times.Once);
            _mockWebBlocker.Verify(webBlocker => webBlocker.SetBlockedWebsites(It.Is<BlockedWebsites>(site => site.BlockedWebsiteUrls.Any(url => url.Url == "example.com"))), Times.Once);
        }

        [TestMethod]
        public void CheckSchedule_NoBlockerCallsWithMulitpleChecks_WhenEventIsNotActive()
        {
            // Arrange
            Event inactiveEvent = new()
            {
                // properties to make the event inactive

                Title = "testing event",
                Start = DateTime.Now.Add(TimeSpan.FromHours(1)),
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
                inactiveEvent
            };

            // Act
            _blockingScheduler.SetConfiguration(events);

            // even with multiple schedule checks there should not be any new blocker function calls
            // since the event has not changed
            _blockingScheduler.CheckSchedule();
            _blockingScheduler.CheckSchedule();
            _blockingScheduler.CheckSchedule();
            _blockingScheduler.CheckSchedule();

            // Assert
            _mockAppBlocker.Verify(appBlocker => appBlocker.SetBlockedApps(
                It.Is<List<string>>(apps => apps.Contains("installLocation"))),
                Times.Never);

            _mockWebBlocker.Verify(webBlocker => webBlocker.SetBlockedWebsites(
                It.Is<BlockedWebsites>(site => site.BlockedWebsiteUrls.Any(url => url.Url == "example.com"))),
                Times.Never);
        }
    }
}
