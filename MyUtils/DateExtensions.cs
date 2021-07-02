using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyUtils
{
    public static class DateExtensions
    {
        public static string ToSimpleString(this DateTimeOffset inputDate) => inputDate.ToString(DateUtil.SimpleYMDHMSFormat);

        public static string ToSimpleString(this DateTime inputDate) => inputDate.ToString(DateUtil.SimpleYMDHMSFormat);

        /// <summary>
        /// shortcut to isostring using <see cref="DateUtil.ISO8601Format"/>
        /// </summary>
        /// <param name="inputDate">The input date.</param>
        /// <returns></returns>
        public static string ToIsoString(this DateTimeOffset inputDate) => inputDate.ToString(DateUtil.ISO8601Format);

        /// <summary>
        /// shortcut to isostring using <see cref="DateUtil.ISO8601Format"/>
        /// </summary>
        /// <param name="inputDate">The input date.</param>
        /// <returns></returns>
        public static string ToIsoString(this DateTime inputDate) => inputDate.ToString(DateUtil.ISO8601Format);

        /// <summary>
        /// Determines whether the time fall within the target period
        /// </summary>
        public static bool IsWithinPeriod(this DateTimeOffset dtToCheck, TimeSpan start, TimeSpan end) => dtToCheck.TimeOfDay.IsWithinPeriod(start, end);

        /// <summary>
        /// Determines whether the time fall within the target period
        /// </summary>
        public static bool IsWithinPeriod(this DateTime dtToCheck, TimeSpan start, TimeSpan end) => dtToCheck.TimeOfDay.IsWithinPeriod(start, end);

        /// <summary>
        /// Determines whether the time fall within the target period
        /// </summary>
        /// <remarks>
        /// Basing on https://stackoverflow.com/a/21343435/4684232
        /// </remarks>
        public static bool IsWithinPeriod(this TimeSpan timeToCheck, TimeSpan start, TimeSpan end)
        {
            if (start <= end)
            {
                // start and stop times are in the same day
                if (timeToCheck >= start && timeToCheck <= end)
                {
                    // current time is between start and stop
                    return true;
                }
            }
            else
            {
                // start and stop times are in different days
                if (timeToCheck >= start || timeToCheck <= end)
                {
                    // current time is between start and stop
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a new DateTime object with the time-part only, while date-part will be change to 1970-01-01
        /// </summary>
        /// <param name="inputDate"></param>
        /// <returns></returns>
        public static DateTime GetTimeOnly(this DateTime inputDate)
        {
            return new DateTime(1970, 1, 1, inputDate.Hour, inputDate.Minute, inputDate.Second);
        }

        /// <summary>
        /// Get a new DateTime object with the time-part only, while date-part will be change to 1970-01-01
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime FromTimeSpanToDateTime(this TimeSpan time)
        {
            return new DateTime(1970, 1, 1, time.Hours, time.Minutes, time.Seconds);
        }

        /// <summary>
        /// Get a new DateTime object with the time-part only, while date-part will be change to 1970-01-01
        /// </summary>
        /// <param name="inputDate"></param>
        /// <returns></returns>
        public static DateTime? GetTimeOnly(this DateTime? inputDate)
        {
            if (!inputDate.HasValue)
            {
                return inputDate;
            }

            return GetTimeOnly(inputDate.Value);
        }

        /// <summary>
        /// Return a new DateTime object representing the end of input date, which is 23:59:59.000
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime EndOfDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddSeconds(-1);
        }

        public static DateTime NormalizeDate(this DateTime dateToBeNormalized)
        {
            DateTime minValue = new DateTime(1970, 1, 1);
            DateTime normalizedDate = minValue;
            if (DateTime.Compare(minValue, dateToBeNormalized) < 0)
            {
                normalizedDate = dateToBeNormalized;
            }
            return normalizedDate;
        }

        public static string FormatDate(this DateTime? dateToBeFormatted)
        {
            if (dateToBeFormatted.HasValue)
            {
                return dateToBeFormatted.Value.ToString("yyyy-MM-dd");
            }
            return null;
        }

        public static string FormatDate(this DateTime? dateToBeFormatted, bool isWeb)
        {
            if (isWeb)
            {
                if (dateToBeFormatted.HasValue)
                {
                    // TODO: Do whatever formating you need to Web
                    //return dateToBeFormatted.Value.ToString(Common.FORMAT_DATETIME_SHORT_DATE_PATTERN);
                    return FormatDate(dateToBeFormatted);
                }
                return null;
            }
            else
            {
                return FormatDate(dateToBeFormatted);
            }
        }

        public static string FormatDateTime(this DateTime dateToBeFormatted)
        {
            if (dateToBeFormatted != null)
            {
                return dateToBeFormatted.ToString("dd/MM/yyyy hh:mm ttt");
            }

            return null;
        }

        /// <summary>
        /// Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC). For JWT token.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ToUnixEpochDate(this DateTime date)
            => (long)Math.Round((date.ToUniversalTime() -
                                 new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .TotalSeconds);

        public static string GetUnixTime(this DateTime dateToBeConverted)
        {
            int unixTime = (int)(dateToBeConverted.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return Convert.ToString(unixTime);
        }

        /// <summary>
        /// Transform a list of Dates into a shorten form, e.g.  "02/01/2018-02/12/2018, 03-04-2018"
        /// </summary>
        public static string DatesToDescription(this IEnumerable<DateTime> dates, bool isWeb)
        {
            if (dates == null || !dates.Any())
            {
                return "";
            }

            if (dates.Count() == 1)
            {
                return FormatDate(dates.First(), isWeb);
            }

            DateTime? startDate = null;
            DateTime? prevDate = null;

            List<string> dateDescList = new List<string>();
            var sortedList = dates.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();
            foreach (var date in sortedList)
            {
                if (!prevDate.HasValue)
                {
                    startDate = date;
                    prevDate = date;
                    continue;
                }

                if (date.AddDays(-1) == prevDate)
                {
                    prevDate = date;
                    continue;
                }
                else
                {
                    dateDescList.Add((prevDate == startDate) ? FormatDate(startDate, isWeb) : FormatDate(startDate, isWeb) + "-" + FormatDate(prevDate, isWeb));

                    startDate = date;
                    prevDate = date;
                }
            }
            dateDescList.Add((prevDate == startDate) ? FormatDate(startDate, isWeb) : FormatDate(startDate, isWeb) + "-" + FormatDate(prevDate, isWeb));

            return string.Join(", ", dateDescList);
        }
    }
}
