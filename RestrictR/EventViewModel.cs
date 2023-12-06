using DataPacketLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictR
{
    public class EventViewModel : INotifyPropertyChanged
    {

        private Event _event;

        public Event Event
        {
            get { return _event; }
            set
            {
                if (_event != value)
                {
                    _event = value;
                    OnPropertyChanged(nameof(Event));
                }
            }
        }

        private DateTimeOffset _startDate;

        public DateTimeOffset StartDate
        {
            get { return _startDate; }
            set
            {
                if(_startDate != value)
                {
                    _startDate = value;
                    UpdateStartDateTime();
                    OnPropertyChanged(nameof(StartDate));
                }
            }
        }

        private TimeSpan _startTime;

        public TimeSpan StartTime
        {
            get { return _startTime; }
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    UpdateStartDateTime();
                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }


        private int _recurrence;

        public int Recurrence
        {
            get { return _recurrence; }
            set {
                if (_recurrence != value)
                {
                    _recurrence = value;
                    UpdateRecurrence();
                    OnPropertyChanged(nameof(Recurrence));
                }
            }
        }

        public class RecurrenceItem
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }

        public IEnumerable<RecurrenceItem> RecurrenceItems
        {
            get { return GetRecurrenceItems(); }
        }

        private void UpdateRecurrence()
        {
            Event.Recurrence = (Event.RecurrenceType)_recurrence;
        }

        private IEnumerable<RecurrenceItem> GetRecurrenceItems()
        {
            yield return new RecurrenceItem() { Text = "None", Value = (int)Event.RecurrenceType.None };
            yield return new RecurrenceItem() { Text = "Daily", Value = (int)Event.RecurrenceType.Daily };
            yield return new RecurrenceItem() { Text = "Weekly", Value = (int)Event.RecurrenceType.Weekly };
            yield return new RecurrenceItem() { Text = "Monthly", Value = (int)Event.RecurrenceType.Monthly };
            yield return new RecurrenceItem() { Text = "Yearly", Value = (int)Event.RecurrenceType.Yearly };
        }


        // sets the default values for the form
        public EventViewModel()
        {
            _startDate = DateTimeOffset.Now;
            _startTime = DateTimeOffset.Now.TimeOfDay;

            Event = new Event
            {
                Duration = TimeSpan.FromHours(1),
                Recurrence = Event.RecurrenceType.None,
                BlockedAppInstallLocations = new List<string>(),
                BlockedSites = new Event.BlockedWebsites()
                {
                    BlockAllSites = false,
                    BlockedWebsiteUrls = new List<string>()
                }
            };

            UpdateStartDateTime();
        }

        private void UpdateStartDateTime()
        {
            Event.Start = _startDate.Date + _startTime;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
