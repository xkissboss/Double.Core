using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Double.Core.Validation
{
    [Serializable]
    public class ValidationException : Exception
    {
        public IList<ValidationResult> ValidationErrors { get; set; }


        public ValidationException(string message) : base(message)

        {
            ValidationErrors = new List<ValidationResult>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="validationErrors">Validation errors</param>
        public ValidationException(string message, IList<ValidationResult> validationErrors) : base(message)

        {
            ValidationErrors = validationErrors;
        }
    }
}
