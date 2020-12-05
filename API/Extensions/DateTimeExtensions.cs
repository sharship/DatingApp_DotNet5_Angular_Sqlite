using System;

namespace Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            
            if(dob.AddYears(age) > today) age -= 1;

            return age;
        }
    }
}