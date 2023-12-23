using System;
using System.ComponentModel.DataAnnotations;

namespace RestrictR.ValidationAttributes
{
    internal class StartDateValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTimeOffset date)
            {
                bool legit = date.Date >= DateTimeOffset.Now.Date;
                return legit;
            }

            return false;
        }
    }
}
