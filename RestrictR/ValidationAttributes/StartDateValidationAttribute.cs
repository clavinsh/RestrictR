using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
