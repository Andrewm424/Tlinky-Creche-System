using System;
using Tlinky.AdminWeb.Models;

namespace Tlinky.AdminWeb.Helpers
{
    public static class FeeCalculator
    {
        public static decimal CalculateMonthlyFee(Child child, SystemSetting settings)
        {
            if (!child.DOB.HasValue) return settings.BaseMonthlyFee;

            var today = DateTime.UtcNow.Date;
            var dob = child.DOB.Value.Date;

            int age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;

            if (age <= 3) return settings.ToddlerFee;
            if (age <= 6) return settings.PreschoolFee;
            return settings.BaseMonthlyFee;
        }
    }
}
