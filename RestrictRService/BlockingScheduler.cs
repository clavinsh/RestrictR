using DataPacketLibrary.Models;
using Serilog;

namespace RestrictRService
{
    public class BlockingScheduler
    {
        private List<Event> events;
        private Event? currentEvent;
        private Timer _timer;
        private readonly IApplicationBlocker _appBlocker;
        private readonly IWebsiteBlocker _webBlocker;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IClock _clock;

        public BlockingScheduler(IApplicationBlocker appBlocker, IWebsiteBlocker webBlocker, IServiceScopeFactory serviceScopeFactory, IClock clock)
        {
            _appBlocker = appBlocker;
            _webBlocker = webBlocker;
            _serviceScopeFactory = serviceScopeFactory;

            events = new();
            _clock = clock;

            UpdateConfiguration();
        }

        // updates the configuration (list of events) by retrieving them from the database
        // through the use of the event controller
        public async void UpdateConfiguration()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var eventController = scope.ServiceProvider.GetRequiredService<EventController>();

            var newConfig = (await eventController.GetEvents()).ToList();
            events = newConfig;

            CheckSchedule();
        }


        // primarily used for testing purposes
        public void SetConfiguration(List<Event> newEvents)
        {
            events = newEvents;
            CheckSchedule();
        }

        // checks and updates the event schedule as needed
        // adds the necessary config to website, software blockers as needed.
        public void CheckSchedule()
        {
            DateTime currentTime = _clock.Now;

            Log.Information("Checking the schedule");

            // Find the next or ongoing event
            Event? nextEvent = FindNextEvent(currentTime);

            // Implement or remove blocking based on event changes
            if (nextEvent != null && IsEventActive(nextEvent, currentTime))
            {
                if (currentEvent == null || !currentEvent.Equals(nextEvent))
                {
                    if (currentEvent != null)
                    {
                        RemoveEventBlocking(currentEvent);
                    }

                    ImplementEventBlocking(nextEvent);
                    currentEvent = nextEvent;
                }
            }
            else if (currentEvent != null)
            {
                RemoveEventBlocking(currentEvent);
                currentEvent = null;
            }

            DateTime nextCheckTime;

            if (currentEvent != null)
            {
                nextCheckTime = currentEvent.Start + currentEvent.Duration;
            }
            else if (nextEvent != null)
            {
                nextCheckTime = nextEvent.Start;
            }
            else
            {
                nextCheckTime = currentTime.AddMinutes(5);
            }

            // Schedule the next check
            ScheduleNextCheck(nextCheckTime);
        }


        private void ScheduleNextCheck(DateTime nextEventStart)
        {
            if (nextEventStart == DateTime.MaxValue) return; // No upcoming events

            var timeUntilNextEvent = nextEventStart.Subtract(_clock.Now);
            if (timeUntilNextEvent <= TimeSpan.Zero) return; // The next event has already started

            _timer?.Dispose(); // Dispose previous timer if exists
            _timer = new Timer(_ => CheckSchedule(), null, timeUntilNextEvent, Timeout.InfiniteTimeSpan);
        }

        // Check if an event is still active based on start time and duration, recurrence
        private static bool IsEventActive(Event configEvent, DateTime currentTime)
        {
            // Non-recurring event, simply check if current time falls within event timeframe
            if (configEvent.Recurrence == Event.RecurrenceType.None)
            {
                return currentTime >= configEvent.Start && currentTime <= configEvent.Start + configEvent.Duration;
            }
            else
            {
                DateTime eventEnd = configEvent.Start + configEvent.Duration;

                return configEvent.Recurrence switch
                {
                    Event.RecurrenceType.Daily => currentTime.TimeOfDay >= configEvent.Start.TimeOfDay
                                                && currentTime.TimeOfDay <= eventEnd.TimeOfDay,

                    Event.RecurrenceType.Weekly => currentTime.DayOfWeek == configEvent.Start.DayOfWeek
                    && currentTime.TimeOfDay >= configEvent.Start.TimeOfDay
                    && currentTime.TimeOfDay <= eventEnd.TimeOfDay,

                    Event.RecurrenceType.Monthly => currentTime.Day == configEvent.Start.Day
                    && currentTime.TimeOfDay >= configEvent.Start.TimeOfDay
                    && currentTime.TimeOfDay <= eventEnd.TimeOfDay,

                    Event.RecurrenceType.Yearly => currentTime.Month == configEvent.Start.Month
                    && currentTime.Day == configEvent.Start.Day
                    && currentTime.TimeOfDay >= configEvent.Start.TimeOfDay
                    && currentTime.TimeOfDay <= eventEnd.TimeOfDay,

                    _ => false,
                };
            }
        }

        // Find the next scheduled event based on the current time
        private Event? FindNextEvent(DateTime currentTime)
        {
            // if there is a situation where the next event is already on going,
            // then return that event
            var ongoingEvent = events.FirstOrDefault(e => IsEventActive(e, currentTime));
            if (ongoingEvent != null)
            {
                return ongoingEvent;
            }

            // if there isn't an ongoing one, then return the next upcoming event
            return events.Where(e => e.Start > currentTime)
                .OrderBy(e => e.Start)
                .FirstOrDefault(e => IsEventActive(e, currentTime));
        }

        private void ImplementEventBlocking(Event configEvent)
        {
            var BlockedAppInstallLocations = configEvent.BlockedApps.Select(app => app.InstallLocation).ToList();

            if (BlockedAppInstallLocations.Count > 0)
            {
                _appBlocker.SetBlockedApps(BlockedAppInstallLocations);
            }

            if (configEvent.BlockedSites != null)
            {
                _webBlocker.SetBlockedWebsites(configEvent.BlockedSites);
            }

            Log.Information("Implemented blocking event with title: {@Title}", configEvent.Title);
        }

        private void RemoveEventBlocking(Event configEvent)
        {
            _appBlocker.RemoveBlockedApps();
            _webBlocker.RemoveBlockedWebsites();
        }
    }
}
