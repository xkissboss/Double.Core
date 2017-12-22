using System;
using System.Collections.Generic;
using System.Text;

namespace Double.Core.Validation
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property)]
    public class DisableValidationAttribute : Attribute
    {

    }
}
