using DataPacketLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RestrictR.ValidationAttributes;
using CommunityToolkit.Mvvm.Input;
using RestrictR.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace RestrictR
{
    public partial class EventViewModel : ObservableValidator
    {
        private IMessenger _messenger;

        // and sets the default values for the inputs
        public EventViewModel(IMessenger messenger)
        {
            _messenger = messenger;

            // used for form invalidation - disabling the primary button by sending a message to the main window
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "HasErrors")
                {
                    _messenger.Send(new FormValidityChangedMessage { IsFormValid = this.IsFormValid });
                }
            };


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

        //public event EventHandler FormSubmissionCompleted;
        //public event EventHandler FormSubmissionFailed;

        public bool IsFormValid => !HasErrors;

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

        [Required]
        [StartDateValidation]
        public DateTimeOffset StartDate
        {
            get { return _startDate; }
            set
            {
                if(_startDate != value)
                {
                    SetProperty(ref _startDate, value, true);
                    UpdateStartDateTime();
                    OnPropertyChanged(nameof(StartDate));

                    //ValidateProperty(StartDate, nameof(StartDate));
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

                    ValidateProperty(StartTime, nameof(StartTime));
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

                    ValidateProperty(Recurrence, nameof(Recurrence));
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

        private void UpdateStartDateTime()
        {
            Event.Start = _startDate.Date + _startTime;
        }


        //public event PropertyChangedEventHandler PropertyChanged;

        //protected new virtual void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        //[RelayCommand]
        //private void Submit()
        //{
        //    ValidateAllProperties();

        //    if (HasErrors)
        //    {
        //        FormSubmissionFailed?.Invoke(this, EventArgs.Empty);
        //    }
        //    else
        //    {
        //        FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);
        //    }

        //}

        //[RelayCommand]
        //private void ShowErrors()
        //{
        //    string msg = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));

        //    //_ = DialogService.ShowMessageDialogAsync("Validation errors", msg);
        //}
    }
}
