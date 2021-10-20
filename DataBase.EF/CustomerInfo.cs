using System;
using System.Collections.Generic;
using System.Text;

namespace DataBase.EF
{
    public class CustomerInfo : Customer
    {
        public string EmployeeName { get; set; }

        public new string test(int x)
        {
            return "CustomerInfo";
        }
    }
}
