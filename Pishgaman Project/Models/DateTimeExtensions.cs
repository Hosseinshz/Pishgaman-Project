using System;
using System.Globalization;

namespace Pishgaman_Project.Models
{
    public static class DateTimeExtensions
    {
        public static string ToPersianDate(this DateTime dateTime)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            int year = persianCalendar.GetYear(dateTime);
            int month = persianCalendar.GetMonth(dateTime);
            int day = persianCalendar.GetDayOfMonth(dateTime);

            // Return the date in a readable Persian format (e.g., yyyy/MM/dd)
            return $"{year}/{month:D2}/{day:D2}";
        }
    }

}
