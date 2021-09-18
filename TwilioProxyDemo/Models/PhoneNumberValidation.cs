using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TwilioProxyDemo.Services;
using Microsoft.Extensions.DependencyInjection;
using PhoneNumbers;

namespace TwilioProxyDemo.Models
{
    /// <summary>
    /// A custom validation class that uses libphonenumber-csharp to verify phone numbers. 
    /// https://github.com/twcclegg/libphonenumber-csharp
    /// </summary>
    public class CustomPhoneNumberValidationAttribute : ValidationAttribute
    {
        private readonly PhoneNumberUtil phoneNumberUtil;

        public CustomPhoneNumberValidationAttribute()
        {
            phoneNumberUtil = PhoneNumberUtil.GetInstance();
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string number = value as string;
            ValidationResult errorResult = new ValidationResult("Invalid number", new[] { validationContext.MemberName });
            
            if (string.IsNullOrWhiteSpace(number) || number?.Length > 20)
                return errorResult;

            var countryCodeProperty = validationContext.ObjectType.GetProperty("CountryCode");
            if (countryCodeProperty == null)
                return errorResult;
                       
            try
            {
                var countryCode = (string)countryCodeProperty.GetValue(validationContext.ObjectInstance);
                var phoneNumber = phoneNumberUtil.Parse(number, countryCode);
                var isValid = phoneNumberUtil.IsValidNumber(phoneNumber);
                if (isValid)
                {
                    return ValidationResult.Success;
                }
                return errorResult;
            }
            catch
            {
                return errorResult;
            }
        }
    }
}
