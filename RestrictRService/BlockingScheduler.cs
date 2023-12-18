using DataPacketLibrary.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public BlockingScheduler(IApplicationBlocker appBlocker, IWebsiteBlocker webBlocker, IServiceScopeFactory serviceScopeFactory)
        {
            _appBlocker = appBlocker;
            _webBlocker = webBlocker;
            _serviceScopeFactory = serviceScopeFactory;

            events = new();
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
        private void CheckSchedule()
        {
            DateTime currentTime = DateTime.Now;

            // Find the next or ongoing event
            Event? nextEvent = FindNextEvent(currentTime);

            // Check if the next event is currently active
            if (nextEvent != null && IsEventActive(nextEvent, currentTime))
            {
                if (currentEvent != null && !Equals(currentEvent, nextEvent))
                {
                    RemoveEventBlocking(currentEvent);
                }

                ImplementEventBlocking(nextEvent);
                currentEvent = nextEvent;
            }
            else if (currentEvent != null)
            {
                // If there's a current event but it's no longer active, remove its blocking
                RemoveEventBlocking(currentEvent);
                currentEvent = null;
            }

            // Optionally, schedule the next check if nextEvent is in the future
            ScheduleNextCheck(nextEvent?.Start ?? DateTime.MaxValue);
        }

        private void ScheduleNextCheck(DateTime nextEventStart)
        {
            if (nextEventStart == DateTime.MaxValue) return; // No upcoming events

            var timeUntilNextEvent = nextEventStart.Subtract(DateTime.Now);
            if (timeUntilNextEvent <= TimeSpan.Zero) return; // The next event has already started

            _timer?.Dispose(); // Dispose previous timer if exists
            _timer = new Timer(_ => CheckSchedule(), null, timeUntilNextEvent, Timeout.InfiniteTimeSpan);
        }

        // Check if an event is still active based on start time and duration, recurrence
        private static bool IsEventActive(Event configEvent,  DateTime currentTime)
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
        }

        private void RemoveEventBlocking(Event configEvent)
        {
            _appBlocker.RemoveBlockedApps();
            _webBlocker.RemoveBlockedWebsites();
        }
    }
}
