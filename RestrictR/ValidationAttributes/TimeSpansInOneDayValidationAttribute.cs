using System;
using System.ComponentModel.DataAnnotations;

namespace RestrictR.ValidationAttributes
{
    internal class TimeSpansInOneDayValidationAttribute : ValidationAttribute
    {
        private string _durationProperty;

        public TimeSpansInOneDayValidationAttribute(string durationProperty)
        {
            _durationProperty = durationProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object instance = validationContext.ObjectInstance;


            TimeSpan start = (TimeSpan)value;
            TimeSpan duration = (TimeSpan)instance.GetType().GetProperty(_durationProperty).GetValue(instance);

            var endTime = start + duration;

            if (endTime.TotalHours > 24)
            {
                return new("Start Time and Duration must not exceed 24 hours.");
            }

            return ValidationResult.Success;
        }
    }
}
