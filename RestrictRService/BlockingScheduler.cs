using DataPacketLibrary;
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
        private ConfigurationPacket configurationPacket;
        private Event? currentEvent;

        public BlockingScheduler(ConfigurationPacket config)
        {
            configurationPacket = config;
        }

        public void UpdateConfiguration(ConfigurationPacket newConfig)
        {
            configurationPacket = newConfig;
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

        // Check if an event is still active based on start time and duration
        private bool IsEventActive(Event configEvent,  DateTime currentTime)
        {
            return currentTime >= configEvent.Start && currentTime <= configEvent.Start + configEvent.Duration;
        }

        // Find the next scheduled event based on the current time
        private Event? FindNextEvent(DateTime currentTime)
        {
            return configurationPacket.Events
                .Where(e => e.Start > currentTime)
                .OrderBy(e => e.Start)
                .FirstOrDefault();
        }

        private void ImplementEventBlocking(Event configEvent)
        {
            throw new NotImplementedException();

            if (configEvent.BlockedAppInstallLocations != null)
            {
                
            }

            if (configEvent.BlockedSites != null)
            {
                
            }
        }

        private void RemoveEventBlocking(Event configEvent)
        {
            throw new NotImplementedException();
        }
    }
}
