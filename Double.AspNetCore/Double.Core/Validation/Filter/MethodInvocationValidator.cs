using Double.Core.Extensions;
using Double.Core.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Double.Core.Validation.Filter
{
    public class MethodInvocationValidator
    {
        private const int MaxRecursiveParameterValidationDepth = 8;

        protected MethodInfo Method { get; private set; }
        protected object[] ParameterValues { get; private set; }
        protected ParameterInfo[] Parameters { get; private set; }
        protected List<ValidationResult> ValidationErrors { get; }

        /// <summary>
        /// Creates a new <see cref="MethodInvocationValidator"/> instance.
        /// </summary>
        public MethodInvocationValidator()
        {

            ValidationErrors = new List<ValidationResult>();
        }

        /// <param name="method">Method to be validated</param>
        /// <param name="parameterValues">List of arguments those are used to call the <paramref name="method"/>.</param>
        public virtual void Initialize(MethodInfo method, object[] parameterValues)
        {
            Method = method;
            ParameterValues = parameterValues;
            Parameters = method.GetParameters();
        }

        /// <summary>
        /// Validates the method invocation.
        /// </summary>
        public void Validate()
        {
            CheckInitialized();

            if (Parameters.IsNullOrEmpty())
            {
                return;
            }

            if (!Method.IsPublic)
            {
                return;
            }

            if (IsValidationDisabled())
            {
                return;
            }

            if (Parameters.Length != ParameterValues.Length)
            {
                throw new ValidationException("方法参数计数与参数计数不匹配!");
            }

            if (ValidationErrors.Any() && HasSingleNullArgument())
            {
                ThrowValidationError();
            }

            for (var i = 0; i < Parameters.Length; i++)
            {
                ValidateMethodParameter(Parameters[i], ParameterValues[i]);
            }

            if (ValidationErrors.Any())
            {
                ThrowValidationError();
            }


        }

        protected virtual void CheckInitialized()
        {
            if (Method == null)
            {
                throw new ValidationException("Method对象没有被初始");
            }
        }

        protected virtual bool IsValidationDisabled()
        {
            if (Method.IsDefined(typeof(EnableValidationAttribute), true))
            {
                return false;
            }

            return ReflectionHelper.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault<DisableValidationAttribute>(Method) != null;
        }

        protected virtual bool HasSingleNullArgument()
        {
            return Parameters.Length == 1 && ParameterValues[0] == null;
        }

        protected virtual void ThrowValidationError()
        {
            throw new ValidationException("方法参数验证不通过", ValidationErrors);
        }

        /// <summary>
        /// Validates given parameter for given value.
        /// </summary>
        /// <param name="parameterInfo">Parameter of the method to validate</param>
        /// <param name="parameterValue">Value to validate</param>
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

            ValidateObjectRecursively(parameterValue, 1);
        }

        protected virtual void ValidateObjectRecursively(object validatingObject, int currentDepth)
        {
            if (currentDepth > MaxRecursiveParameterValidationDepth)
            {
                return;
            }

            if (validatingObject == null)
            {
                return;
            }

            SetDataAnnotationAttributeErrors(validatingObject);

            //Validate items of enumerable
            if (validatingObject is IEnumerable && !(validatingObject is IQueryable))
            {
                foreach (var item in (validatingObject as IEnumerable))
                {
                    ValidateObjectRecursively(item, currentDepth + 1);
                }
            }

            //Custom validations
            (validatingObject as ICustomValidate)?.AddValidationErrors(new CustomValidationContext(ValidationErrors));


            //Do not recursively validate for enumerable objects
            if (validatingObject is IEnumerable)
            {
                return;
            }

            var validatingObjectType = validatingObject.GetType();

            //Do not recursively validate for primitive objects
            if (TypeHelper.IsPrimitiveExtendedIncludingNullable(validatingObjectType))
            {
                return;
            }

            //var properties = TypeDescriptor.GetProperties(validatingObject).Cast<PropertyDescriptor>();
            //foreach (var property in properties)
            //{
            //    if (property.Attributes.OfType<DisableValidationAttribute>().Any())
            //    {
            //        continue;
            //    }

            //    ValidateObjectRecursively(property.GetValue(validatingObject), currentDepth + 1);
            //}
        }

        /// <summary>
        /// Checks all properties for DataAnnotations attributes.
        /// </summary>
        protected virtual void SetDataAnnotationAttributeErrors(object validatingObject)
        {
            var properties = TypeDescriptor.GetProperties(validatingObject).Cast<PropertyDescriptor>();
            foreach (var property in properties)
            {
                var validationAttributes = property.Attributes.OfType<ValidationAttribute>().ToArray();
                if (validationAttributes.IsNullOrEmpty())
                {
                    continue;
                }

                var validationContext = new ValidationContext(validatingObject)
                {
                    DisplayName = property.DisplayName,
                    MemberName = property.Name
                };

                foreach (var attribute in validationAttributes)
                {
                    var result = attribute.GetValidationResult(property.GetValue(validatingObject), validationContext);
                    if (result != null)
                    {
                        ValidationErrors.Add(result);
                    }
                }
            }

            if (validatingObject is IValidatableObject)
            {
                var results = (validatingObject as IValidatableObject).Validate(new ValidationContext(validatingObject));
                ValidationErrors.AddRange(results);
            }
        }
    }
}
