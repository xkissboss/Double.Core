using System;
using System.Collections.Generic;
using System.Text;

namespace Double.Core.Domain.Entities
{
    public interface IExtendableObject
    {
        string ExtensionData { get; set; }
    }
}
