using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Double.Core.Validation
{
    public class CustomValidationContext
    {
        /// <summary>
        /// List of validation results (errors). Add validation errors to this list.
        /// </summary>
        public List<ValidationResult> Results { get; }


        public CustomValidationContext(List<ValidationResult> results)
        {
            Results = results;
        }
    }
}
