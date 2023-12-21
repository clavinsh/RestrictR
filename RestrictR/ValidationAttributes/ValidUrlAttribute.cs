using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestrictR.ValidationAttributes
{
    internal class ValidUrlAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                string pattern = @"^www\..+\..+$";

                // Check if it's a well-formed URI
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    return ValidationResult.Success;
                }

                else if (Regex.IsMatch(url, pattern))
                {
                    return ValidationResult.Success;
                }

                // Regex for URLs without scheme (e.g., www.example.com)
                else
                {
                    return new ValidationResult("Invalid URL format.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
