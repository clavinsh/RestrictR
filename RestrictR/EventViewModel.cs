using CommunityToolkit.Mvvm.ComponentModel;
using DataPacketLibrary;
using DataPacketLibrary.Models;
using RestrictR.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictR
{
    public class EventViewModel : ObservableValidator
    {
        private string _title;
        [Required]
        public string Title
        {
            get { return _title; }
            set
            { 
                SetProperty(ref _title, value, true);
            }
        }

        private DateTimeOffset _startDate;
        [Required]
        [StartDateValidation]
        public DateTimeOffset StartDate
        {
            get { return _startDate; }
            set
            {
                SetProperty(ref _startDate, value, true);
            }
        }


        private TimeSpan _startTime;
        public TimeSpan StartTime
        {
            get { return _startTime; }
            set 
            {
                SetProperty(ref _startTime, value, true);
            }
        }


        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                SetProperty(ref _duration, value, true);
            }
        }


        private int _recurrence;
        public int Recurrence
        {
            get { return _recurrence; }
            set
            {
                if (SetProperty(ref _recurrence, value, true))
                {
                    RecurrenceType = (Event.RecurrenceType)Enum.ToObject(typeof(DataPacketLibrary.Models.Event.RecurrenceType), _recurrence);
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

        private static IEnumerable<RecurrenceItem> GetRecurrenceItems()
        {
            yield return new RecurrenceItem() { Text = "None", Value = (int)Event.RecurrenceType.None };
            yield return new RecurrenceItem() { Text = "Daily", Value = (int)Event.RecurrenceType.Daily };
            yield return new RecurrenceItem() { Text = "Weekly", Value = (int)Event.RecurrenceType.Weekly };
            yield return new RecurrenceItem() { Text = "Monthly", Value = (int)Event.RecurrenceType.Monthly };
            yield return new RecurrenceItem() { Text = "Yearly", Value = (int)Event.RecurrenceType.Yearly };
        }

        private Event.RecurrenceType _recurrenceType;
        public Event.RecurrenceType RecurrenceType
        {
            get { return _recurrenceType; }
            set
            {
                if (SetProperty(ref _recurrenceType, value))
                {
                    Recurrence = (int)value;
                }
            }
        }

        // used to retrieve all of the apps installed on the computer
        public List<ApplicationInfo> Apps = new();
        // a collection of what the user sees when filtering for different apps in the form
        public ObservableCollection<ApplicationInfo> AppsFiltered;
        // a collection of apps that the user has selected for blocking
        private ObservableCollection<ApplicationInfo> _blockedApplications;
        public ObservableCollection<ApplicationInfo> BlockedApplications
        { 
            get { return _blockedApplications; }
            set
            {
                SetProperty(ref _blockedApplications, value, true);
            }
        }




        private IEnumerable<ValidationResult> validationErrors;
        public IEnumerable<ValidationResult> ValidationErrors
        {
            get { return validationErrors; }
            set
            {
                SetProperty(ref validationErrors, value);
            }
        }

        private void SetValidationErrors(object sender, DataErrorsChangedEventArgs e)
        {
            ValidationErrors = GetErrors();
        }


        public EventViewModel()
        {
            ErrorsChanged += SetValidationErrors;

            StartDate = DateTimeOffset.Now;
            StartTime = DateTimeOffset.Now.TimeOfDay;
            Duration = TimeSpan.FromHours(1);
            RecurrenceType = Event.RecurrenceType.None;

            Apps = ApplicationRetriever.GetInstalledApplicationsFromRegistry();
            AppsFiltered = new ObservableCollection<ApplicationInfo>(Apps);
            BlockedApplications = new ObservableCollection<ApplicationInfo>();
        }
    }
}
