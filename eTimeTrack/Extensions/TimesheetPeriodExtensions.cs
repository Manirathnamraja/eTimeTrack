using eTimeTrack.Models;
using System;

namespace eTimeTrack.Extensions
{
    public static class TimesheetPeriodExtensions
    {
        public static string GetStartEndDates(this TimesheetPeriod period)
        {
            return period.StartDate.ToString("dd-MMM-yyyy") + " - " + period.EndDate.ToString("dd-MMM-yyyy");
        }

        public static string ToDateStringGeneral(this DateTime dateTime)
        {
            return dateTime.ToString("dd-MMM-yyyy");
        }

        public static string ToDateTimeStringGeneral(this DateTime dateTime)
        {
            return dateTime.ToString("dd-MMM-yyyy hh:mm:ss");
        }

        public static string ToStringDateAndTimeReverse(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd_hh-mm-ss");
        }
    }
}