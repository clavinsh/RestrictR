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
using Microsoft.UI.Xaml;
using System.Runtime.CompilerServices;

namespace RestrictR
{
    public partial class EventViewModel : ObservableValidator
    {
        public event EventHandler FormSubmissionCompleted;
        public event EventHandler FormSubmissionFailed;

        //private IDialogService DialogService;

        // and sets the default values for the inputs
        public EventViewModel()
        {
            ErrorsChanged += SetValidationErrors;
            //_messenger = messenger;

            // used for form invalidation - disabling the primary button by sending a message to the main window
            //PropertyChanged += (s, e) =>
            //{
            //    if (e.PropertyName == "HasErrors")
            //    {
            //        _messenger.Send(new FormValidityChangedMessage { IsFormValid = this.IsFormValid });
            //    }
            //};


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
            //DialogService = dialogService;
        }

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
                }
            }
        }

        private string validationErrorsMessage;
        public string ValidationErrorsMessage
        {
            get { return validationErrorsMessage; }
            set
            {
                if (validationErrorsMessage != value)
                {
                    SetProperty(ref validationErrorsMessage, value);
                }
            } 
        }
        //=> GetErrors();


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

        private INotifyDataErrorInfo dataErrorSource;
        //private void Profile_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        //{
        //    var newDataErrorSource = args.NewValue as INotifyDataErrorInfo;
        //    if (dataErrorSource is not null && dataErrorSource != newDataErrorSource)
        //    {
        //        dataErrorSource.ErrorsChanged -= ProfileErrorsChanged;
        //    }
        //    dataErrorSource = newDataErrorSource;
        //    if (dataErrorSource is not null)
        //    {
        //        dataErrorSource.ErrorsChanged += ProfileErrorsChanged;
        //    }
        //}

        private void SetValidationErrors(object sender, DataErrorsChangedEventArgs e)
        {
            ValidationErrorsMessage = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));
        }


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
        //    string message = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));

        //    //_ = DialogService.ShowMessageDialogAsync("Validation errors", message);
        //}

        //public new event PropertyChangedEventHandler PropertyChanged;

        //private new void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //    => PropertyChanged?.Invoke(this, new(propertyName));

        //private void ValidateStartDate()
        //{
        //    var errors = new List<string>();

        //    if (StartDate.Date >= DateTimeOffset.Now.Date)
        //        errors.Add("Start date must be specified as the current day at the earliest, if no recurrence is present");

        //    SetErrors()
        //}

        //        private void SetErrors(string key, ICollection<string> errors)
        //{
        //    if (errors.Any())
        //        _validationErrors[key] = errors;
        //    else
        //        _ = _validationErrors.Remove(key);

        //    OnErrorsChanged(key);
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
