using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DST_Playground
{
    public class DST
    {
        const string CEST_ID = "Central Europe Standard Time";

        /// <summary>
        /// Write info text in the console about time transition dates 
        /// for all system time zones
        /// </summary>
        /// <param name="year"></param>
        public static void ConsoleTransitionTimes(int year)
        {
            // Instantiate DateTimeFormatInfo object for month names
            DateTimeFormatInfo dateFormat = CultureInfo.CurrentCulture.DateTimeFormat;

            // Get and iterate time zones on local computer
            ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo timeZone in timeZones)
            {
                Console.WriteLine("{0}:", timeZone.StandardName);
                TimeZoneInfo.AdjustmentRule[] adjustments = timeZone.GetAdjustmentRules();
                int startYear = year;
                int endYear = startYear;

                if (adjustments.Length == 0)
                {
                    Console.WriteLine("   No adjustment rules.");
                }
                else
                {
                    TimeZoneInfo.AdjustmentRule adjustment = GetAdjustment(adjustments, year);
                    if (adjustment == null)
                    {
                        Console.WriteLine("   No adjustment rules available for this year.");
                        continue;
                    }
                    TimeZoneInfo.TransitionTime startTransition, endTransition;

                    // Determine if starting transition is fixed 
                    startTransition = adjustment.DaylightTransitionStart;
                    // Determine if starting transition is fixed and display transition info for year
                    if (startTransition.IsFixedDateRule)
                        Console.WriteLine("   Begins on {0} {1} at {2:t}",
                                          dateFormat.GetMonthName(startTransition.Month),
                                          startTransition.Day,
                                          startTransition.TimeOfDay);
                    else
                        DisplayTransitionInfo(startTransition, startYear, "Begins on");

                    // Determine if ending transition is fixed and display transition info for year
                    endTransition = adjustment.DaylightTransitionEnd;

                    // Does the transition back occur in an earlier month (i.e., 
                    // the following year) than the transition to DST? If so, make
                    // sure we have the right adjustment rule.
                    if (endTransition.Month < startTransition.Month)
                    {
                        endTransition = GetAdjustment(adjustments, year + 1).DaylightTransitionEnd;
                        endYear++;
                    }

                    if (endTransition.IsFixedDateRule)
                    {
                        Console.WriteLine("   Ends on {0} {1} at {2:t}",
                                          dateFormat.GetMonthName(endTransition.Month),
                                          endTransition.Day,
                                          endTransition.TimeOfDay);
                    }
                    else
                        DisplayTransitionInfo(endTransition, endYear, "Ends on");
                }
            }
        }

        public static TimeZoneInfo.AdjustmentRule GetAdjustment(TimeZoneInfo.AdjustmentRule[] adjustments, int year)
        {
            // Iterate adjustment rules for time zone
            foreach (TimeZoneInfo.AdjustmentRule adjustment in adjustments)
            {
                // Determine if this adjustment rule covers year desired
                if (adjustment.DateStart.Year <= year && adjustment.DateEnd.Year >= year)
                    return adjustment;
            }
            return null;
        }

        public static void DisplayTransitionInfo(TimeZoneInfo.TransitionTime transition, int year, string label)
        {
            // For non-fixed date rules, get local calendar
            Calendar cal = CultureInfo.CurrentCulture.Calendar;
            // Get first day of week for transition
            // For example, the 3rd week starts no earlier than the 15th of the month
            int startOfWeek = transition.Week * 7 - 6;
            // What day of the week does the month start on?
            int firstDayOfWeek = (int)cal.GetDayOfWeek(new DateTime(year, transition.Month, 1));
            // Determine how much start date has to be adjusted
            int transitionDay;
            int changeDayOfWeek = (int)transition.DayOfWeek;

            if (firstDayOfWeek <= changeDayOfWeek)
                transitionDay = startOfWeek + (changeDayOfWeek - firstDayOfWeek);
            else
                transitionDay = startOfWeek + (7 - firstDayOfWeek + changeDayOfWeek);

            // Adjust for months with no fifth week
            if (transitionDay > cal.GetDaysInMonth(year, transition.Month))
                transitionDay -= 7;

            Console.WriteLine("   {0} {1}, {2:d} at {3:t}",
                              label,
                              transition.DayOfWeek,
                              new DateTime(year, transition.Month, transitionDay),
                              transition.TimeOfDay);
        }

        /// <summary>
        /// Gives you a DateTime object representing TransitionTime object
        /// </summary>
        /// <param name="transitionTime">TimeZoneInfo.TransitionTime object representing a transition time date</param>
        /// <param name="year">transition time date's year</param>
        /// <returns>DateTime object representing given TransitionTime object</returns>
        public static DateTime GetAdjustmentDate(TimeZoneInfo.TransitionTime transitionTime, int year)
        {
            if (transitionTime.IsFixedDateRule)
            {
                return new DateTime(year, transitionTime.Month, transitionTime.Day);
            }
            else
            {
                // For non-fixed date rules, get local calendar
                Calendar cal = CultureInfo.CurrentCulture.Calendar;
                // Get first day of week for transition
                // For example, the 3rd week starts no earlier than the 15th of the month
                int startOfWeek = transitionTime.Week * 7 - 6;
                // What day of the week does the month start on?
                int firstDayOfWeek = (int)cal.GetDayOfWeek(new DateTime(year, transitionTime.Month, 1));
                // Determine how much start date has to be adjusted
                int transitionDay;
                int changeDayOfWeek = (int)transitionTime.DayOfWeek;

                if (firstDayOfWeek <= changeDayOfWeek)
                    transitionDay = startOfWeek + (changeDayOfWeek - firstDayOfWeek);
                else
                    transitionDay = startOfWeek + (7 - firstDayOfWeek + changeDayOfWeek);

                // Adjust for months with no fifth week
                if (transitionDay > cal.GetDaysInMonth(year, transitionTime.Month))
                    transitionDay -= 7;

                return new DateTime(year, transitionTime.Month, transitionDay, transitionTime.TimeOfDay.Hour, transitionTime.TimeOfDay.Minute, transitionTime.TimeOfDay.Second);
            }
        }

        /// <summary>
        /// Returns next time transition date from given date. If date equeals a time transition date it is returned then.
        /// For example for 2017 March change date is 26 March 2017 CEST and result of GetNextTransition(new DateTime(2017, 3, 26), "Central Europe Standard Time")
        /// will be DateTime object for date 26 March 2017
        /// </summary>
        /// <param name="asOfTime">Date starting the check from</param>
        /// <param name="timeZone">Timezone agains the check is made</param>
        /// <returns>DateTime object representing the first date after asOfTime date that is a time transition date or NULL if no time transition in this timeZone</returns>
        public static DateTime? GetNextTransition(DateTime asOfTime, TimeZoneInfo timeZone)
        {
            TimeZoneInfo.AdjustmentRule[] adjustments = timeZone.GetAdjustmentRules();
            if (adjustments.Length == 0)
            {
                // if no adjustment then no transition date exists
                return null;
            }

            int year = asOfTime.Year;
            TimeZoneInfo.AdjustmentRule adjustment = null;
            foreach (TimeZoneInfo.AdjustmentRule adj in adjustments)
            {
                // Determine if this adjustment rule covers year desired
                if (adj.DateStart.Year <= year && adj.DateEnd.Year >= year)
                {
                    adjustment = adj;
                    break;
                }
            }

            if (adjustment == null)
            {
                // no adjustment found so no transition date exists in the range
                return null;
            }


            DateTime dtAdjustmentStart = GetAdjustmentDate(adjustment.DaylightTransitionStart, year);
            DateTime dtAdjustmentEnd = GetAdjustmentDate(adjustment.DaylightTransitionEnd, year);


            if (dtAdjustmentStart.Date >= asOfTime.Date)
            {
                // if adjusment start date is greater than asOfTime date then this should be the next transition date
                return dtAdjustmentStart;
            }
            else if (dtAdjustmentEnd.Date >= asOfTime.Date)
            {
                // otherwise adjustment end date should be the next transition date
                return dtAdjustmentEnd;
            }
            else
            {
                // then it should be the next year's DaylightTransitionStart

                year++;
                foreach (TimeZoneInfo.AdjustmentRule adj in adjustments)
                {
                    // Determine if this adjustment rule covers year desired
                    if (adj.DateStart.Year <= year && adj.DateEnd.Year >= year)
                    {
                        adjustment = adj;
                        break;
                    }
                }

                dtAdjustmentStart = GetAdjustmentDate(adjustment.DaylightTransitionStart, year);
                return dtAdjustmentStart;
            }
        }

        /// <summary>
        /// Returns next time transition date from given date. If date equeals a time transition date it is returned then.
        /// For example for 2017 March change date is 26 March 2017 CEST and result of GetNextTransition(new DateTime(2017, 3, 26), "Central Europe Standard Time")
        /// will be DateTime object for date 26 March 2017
        /// </summary>
        /// <param name="asOfTime">Date starting the check from</param>
        /// <param name="timeZone">Timezone agains the check is made</param>
        /// <returns>DateTime object representing the first date after asOfTime date that is a time transition date or NULL if no time transition in this timeZone</returns>
        public static DateTime? GetNextTransition(DateTime asOfTime, string timeZone)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            if (timeZoneInfo == null)
                return null;

            return GetNextTransition(asOfTime, timeZoneInfo);
        }

        /// <summary>
        /// This function actually help to determine how many hours the day has.
        /// in case of last sunday of march result should be 23 and 
        /// in case of last sunday ofoctober result should be 25
        /// </summary>
        /// <param name="date">Date which hours count is checked</param>
        /// <returns>hours of the given date</returns>
        public static int GetDayHours(DateTime date, string timeZoneId)
        {
            string tzId = string.IsNullOrEmpty(timeZoneId) ? CEST_ID : timeZoneId;
            int month = date.Month;
            DateTime transitionDate = GetNextTransition(date, tzId) ?? DateTime.MinValue;
            bool isTransitionDate = transitionDate != DateTime.MinValue && transitionDate.Date == date.Date ? true : false;

            // March at the last sunday time is moved with one hour forward 
            // that means the day will actually have 23 hours 

            // October at the last sunday time is moved with one hour backward 
            // that means the day will actually have 25 hours  
            return isTransitionDate && month == 3 ? 23 : isTransitionDate && month == 10 ? 25 : 24;
        }
    }
}
