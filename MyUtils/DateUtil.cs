using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MyUtils
{
    public class DateUtil
    {
        public static TimeZoneInfo EstTimeZone => TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Eastern Standard Time" : "America/New_York");
        public static DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime UtcToday => new DateTime(DateTimeOffset.UtcNow.Date.Ticks, DateTimeKind.Utc);

        public const string SimpleYMDHMSFormat = "yyyy-MM-dd HH:mm:ss";

        // ISO Datetime formats
        public const string ISO8601Format = "yyyy-MM-ddTHH:mm:ss.fff";
        public const string ISO8601DateFormat = "yyyy-MM-dd";
        public const string ISO8601TimeFormat = "HH:mm:ss.fff";
        public const string ShortTimeFormat = "HH:mm";

        public const string FilenameFormat = "yyyyMMddHHmmssfff";

        // Datetime Format for C# (slash is encapsulated with '')
        public const string FORMAT_DATETIME_FULL_DATETIME_PATTERN = "dd'/'MM'/'yyyy HH:mm";
        public const string FORMAT_DATETIME_LONG_DATE_PATTERN = "dd'/'MM'/'yyyy (ddd)";
        public const string FORMAT_DATETIME_SHORT_DATE_PATTERN = "dd'/'MM'/'yyyy";
        public const string FORMAT_DATETIME_TIME_12_PATTERN = "hh:mm tt";
        public const string FORMAT_DATETIME_TIME_24_PATTERN = "HH:mm";

        // Datetime Format for Javascript
        public const string FORMAT_DATETIMEPICKER_PARSE = "DD/MM/YYYY HH:mm";
        public const string FORMAT_DATEPICKER_PARSE = "DD/MM/YYYY";
        //public const string FORMAT_TIMEPICKER_12_PARSE = "hh:mm A";
        //public const string FORMAT_TIMEPICKER_24_PARSE = "HH:mm";

        //public const string FORMAT_REPORT_DATETIMEPICKER_EN = "dd-MMM-yy";
        //public const string FORMAT_REPORT_DATETIMEPICKER = @"dd'/'MM'/'yyyy HH:mm:ss.ttt";


        /// <summary>
        /// Timediff mocked. Change on this value will be persisted to DB immediately.
        /// </summary>
        public virtual TimeSpan Timediff { get; set; }
        public virtual DateTime Now { get { return DateTime.Now.Add(Timediff); } }
        public virtual DateTime UtcNow { get { return DateTime.UtcNow.Add(Timediff); } }
        public DateTime Today { get { return Now.Date; } }

        /// <summary>
        /// Minimum date accepted in the system
        /// </summary>
        public static DateTime MIN
        {
            get
            {
                return System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            }
        }

        /// <summary>
        /// Maximum date accepted in the system
        /// </summary>
        public static DateTime MAX
        {
            get
            {
                return System.Data.SqlTypes.SqlDateTime.MaxValue.Value;
            }
        }

        /// <summary>
        /// Self-defined EMPTY
        /// </summary>
        public static DateTime EMPTY
        {
            get
            {
                return new DateTime(1900, 1, 1);
            }
        }

        /// <summary>
        /// Return Max <paramref name="dateTimes"/> in UTC
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public static DateTime GetMaxUtc(params DateTimeOffset[] dateTimes) => GetMaxUtc(dateTimes.Select(dt => dt.DateTime).ToArray());

        /// <summary>
        /// Return Min <paramref name="dateTimes"/> in UTC
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public static DateTime GetMinUtc(params DateTimeOffset[] dateTimes) => GetMinUtc(dateTimes.Select(dt => dt.DateTime).ToArray());

        /// <summary>
        /// Return Max <paramref name="dateTimes"/> in UTC
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public static DateTime GetMaxUtc(params DateTime[] dateTimes)
        {
            if (dateTimes == null || !dateTimes.Any())
            {
                return DateTime.MinValue;
            }

            return new DateTime(dateTimes.Select(dt => dt.ToUniversalTime().Ticks).Max(), DateTimeKind.Utc);
        }

        /// <summary>
        /// Return Max <paramref name="dateTimes"/>
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public static DateTime GetMax(params DateTime[] dateTimes)
        {
            if (dateTimes == null || !dateTimes.Any())
            {
                return DateTime.MinValue;
            }

            var kinds = dateTimes.GroupBy(x=>x.Kind).Select(x=>x.Key).ToArray();
            if (kinds.Length != 1)
            {
                throw new ArgumentException($"multiple kinds are not supported. kinds found: {kinds.ToJson()}", nameof(dateTimes));
            }

            return new DateTime(dateTimes.Select(dt => dt.Ticks).Max(), kinds.Single());
        }

        /// <summary>
        /// Return Min <paramref name="dateTimes"/> in UTC
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public static DateTime GetMinUtc(params DateTime[] dateTimes)
        {
            if (dateTimes == null || !dateTimes.Any())
            {
                return DateTime.MinValue;
            }

            return new DateTime(dateTimes.Select(dt => dt.ToUniversalTime().Ticks).Min(), DateTimeKind.Utc);
        }

        /// <summary>
        /// Return Min <paramref name="dateTimes"/>
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public static DateTime GetMin(params DateTime[] dateTimes)
        {
            if (dateTimes == null || !dateTimes.Any())
            {
                return DateTime.MinValue;
            }

            var kinds = dateTimes.GroupBy(x => x.Kind).Select(x => x.Key).ToArray();
            if (kinds.Length != 1)
            {
                throw new ArgumentException($"multiple kinds are not supported. kinds found: {kinds.ToJson()}", nameof(dateTimes));
            }

            return new DateTime(dateTimes.Select(dt => dt.Ticks).Min(), kinds.Single());
        }

        public static string GetCurrentUnixTime()
        {
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            return Convert.ToString(unixTime);
        }

        public static DateTime? GetDateTime(string dateStr)
        {
            if (!string.IsNullOrEmpty(dateStr))
            {
                var dateArray = dateStr.Split('/');
                if (dateArray.Length > 2)
                {
                    int year = Convert.ToInt16(dateArray[2]);
                    int month = Convert.ToInt16(dateArray[1]);
                    int day = Convert.ToInt16(dateArray[0]);
                    DateTime date = new DateTime(year, month, day);
                    return date;
                }
            }
            return null;
        }

        public static DateTime GetDateTimeNotNull(string dateStr)
        {
            if (!string.IsNullOrEmpty(dateStr))
            {
                var dateArray = dateStr.Split('/');
                if (dateArray.Length > 2)
                {
                    int year = Convert.ToInt16(dateArray[2]);
                    int month = Convert.ToInt16(dateArray[1]);
                    int day = Convert.ToInt16(dateArray[0]);
                    DateTime date = new DateTime(year, month, day);
                    return date;
                }
            }
            return DateTime.MinValue;
        }

    }
}
