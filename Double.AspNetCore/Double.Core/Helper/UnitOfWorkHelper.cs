using Double.Core.Uow;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Double.Core.Helper
{
    internal static class UnitOfWorkHelper
    {
        /// <summary>
        /// Returns true if given method has UnitOfWorkAttribute attribute.
        /// </summary>
        /// <param name="memberInfo">Method info to check</param>
        public static bool HasUnitOfWorkAttribute(MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(UnitOfWorkAttribute), true);
        }
    }
}
