using System;
using System.Collections.Generic;
using System.Text;

namespace Double.Core.Validation
{
    public interface ICustomValidate
    {
        /// <summary>
        /// This method is used to validate the object.
        /// </summary>
        /// <param name="context">Validation context.</param>
        void AddValidationErrors(CustomValidationContext context);
    }
}
