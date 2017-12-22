using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Double.Core.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Double.Core.Helper;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Double.Core.Validation.Filter
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {

            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                MethodInfo methodInfo = controllerActionDescriptor.MethodInfo;
                if (!context.ModelState.IsValid)
                {
                    foreach (var state in context.ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            ValidationErrors.Add(new ValidationResult(error.ErrorMessage, new[] { state.Key }));
                        }
                    }
                    Validate(context, methodInfo);
                }

            }



        }



        protected List<ValidationResult> ValidationErrors = new List<ValidationResult>();

        protected void Validate(ActionExecutingContext actionContext, MethodInfo methodInfo)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.IsNullOrEmpty())
            {
                return;
            }

            if (!methodInfo.IsPublic)
            {
                return;
            }

            if (IsValidationDisabled(methodInfo))
            {
                return;
            }
            object[] parameterValues = GetParameterValues(actionContext, methodInfo);
            if (parameters.Length != parameterValues.Length)
            {
                throw new Exception("方法参数计数与传递的参数计数不匹配!");
            }

            if (ValidationErrors.Any() && (parameters.Length == 1 && parameterValues[0] == null))
            {
                ThrowValidationError();
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                ValidateMethodParameter(parameters[i], parameterValues[i]);
            }

            if (ValidationErrors.Any())
            {
                ThrowValidationError();
            }

        }




        /// <summary>
        /// 验证参数是否允许为空
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <param name="parameterValue"></param>
        protected virtual void ValidateMethodParameter(ParameterInfo parameterInfo, object parameterValue)
        {
            if (parameterValue == null)
            {
                if (!parameterInfo.IsOptional &&
                    !parameterInfo.IsOut &&
                    !TypeHelper.IsPrimitiveExtendedIncludingNullable(parameterInfo.ParameterType, includeEnums: true))
                {
                    ValidationErrors.Add(new ValidationResult(parameterInfo.Name + " is null!", new[] { parameterInfo.Name }));
                }

                return;
            }

            (parameterValue as ICustomValidate)?.AddValidationErrors(new CustomValidationContext(ValidationErrors));
        }



        protected virtual void ThrowValidationError()
        {
            throw new ValidationException("方法参数验证不通过", ValidationErrors);
        }

        protected bool IsValidationDisabled(MethodInfo methodInfo)
        {
            if (methodInfo.IsDefined(typeof(EnableValidationAttribute), true))
            {
                return false;
            }
            return ReflectionHelper.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault<DisableValidationAttribute>(methodInfo) != null;
        }

        protected virtual object[] GetParameterValues(ActionExecutingContext context, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var parameterValues = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                parameterValues[i] = context.ActionArguments.GetOrDefault(parameters[i].Name);
            }

            return parameterValues;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
