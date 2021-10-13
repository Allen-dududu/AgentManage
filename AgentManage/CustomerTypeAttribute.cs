using AgentManage.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgentManage
{
    public class CustomerTypeAttribute : ValidationAttribute
    {
        public CustomerTypeAttribute()
        {
        }

        public string GetErrorMessage() =>
        $"客户类型不正确.";

        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
            var Customer = (CustomerRequest)validationContext.ObjectInstance;


            if (Customer.Type != CustomerType.A && Customer.Type != CustomerType.B && Customer.Type != CustomerType.C)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }
    }
}
