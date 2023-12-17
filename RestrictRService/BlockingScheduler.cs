using DataPacketLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictRService
{
    public class BlockingScheduler
    {
        //private Timer timer;
        private List<Event> events;
        private Event? currentEvent;

        private readonly ApplicationBlocker _appBlocker;
        private readonly WebsiteBlocker _webBlocker;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BlockingScheduler(ApplicationBlocker appBlocker, WebsiteBlocker webBlocker, IServiceScopeFactory serviceScopeFactory)
        {
            _appBlocker = appBlocker;
            _webBlocker = webBlocker;
            _serviceScopeFactory = serviceScopeFactory;

            events = new();
        }

        public void UpdateConfiguration(List<Event> newConfig)
        {
            events = newConfig;
            CheckSchedule();
        }

        // checks and updates the event schedule as needed
        // adds the necessary config to website, software blockers as needed.
        private void CheckSchedule()
        {
            DateTime currentTime = DateTime.Now;

            // If the current event is still active, nothing has changed
            if (currentEvent != null && IsEventActive(currentEvent, currentTime))
            {
                return;
            }

            // The current is over or null, need to find the next one
            Event? nextevent = FindNextEvent(currentTime);
            if (currentEvent != null)
            {
                RemoveEventBlocking(currentEvent);
            }

            // Sets the next event as the current one and implements it
            currentEvent = nextevent;
            if (currentEvent != null)
            {
                ImplementEventBlocking(currentEvent);
            }
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
