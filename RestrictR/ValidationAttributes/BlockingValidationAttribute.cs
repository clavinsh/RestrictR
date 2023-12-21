using DataPacketLibrary.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictR.ValidationAttributes
{
    // used to check if at least one blocking configuration is set
    // otherwise no point in send the event in such a state where nothing gets blocked
    // this attribute will only work exclusively on the EventViewModel class and the specified properties
    internal class BlockingValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var viewModel = (EventViewModel)validationContext.ObjectInstance;

            bool isBlockingSet = viewModel.BlockAllSites 
                || (viewModel.BlockedUrls != null && viewModel.BlockedUrls.Any())
                || (viewModel.BlockedApplications != null && viewModel.BlockedApplications.Any());

            if (!isBlockingSet)
            {
                return new("At least one blocking condition must be set");
            }

            return ValidationResult.Success;
        }
    }
}
