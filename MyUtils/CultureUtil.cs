using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MyUtils
{
    public static class CultureUtil
    {
        /// <summary>
        /// Try-Get style Get Culture
        /// </summary>
        /// <remarks>See https://stackoverflow.com/a/13723966</remarks>
        /// <param name="cultureCode"></param>
        /// <param name="DefaultCultureCode"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static bool TryGetCultureInfo(string cultureCode, string DefaultCultureCode, out CultureInfo culture)
        {
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureCode);
                return true;
            }
            catch (Exception ex) when (ex is CultureNotFoundException || ex is ArgumentNullException)
            {
                if (DefaultCultureCode == null)
                    culture = CultureInfo.CurrentCulture;
                else
                {
                    try
                    {
                        culture = CultureInfo.GetCultureInfo(DefaultCultureCode);
                    }
                    catch (CultureNotFoundException)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                }
            }
            return false;
        }
    }
}
