using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MyUtils
{
    public static class JsonUtils
    {
        /// <summary>
        /// Default usable settings for <see cref="System.Text.Json"/>
        /// </summary>
        public static JsonSerializerOptions DefaultJsonSerializerOptions => new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        /// <summary>
        /// Populate properties from <paramref name="value"/> to <paramref name="target"/>.
        /// See https://stackoverflow.com/a/30220811/4684232
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="target"></param>
        [Obsolete("Use native method target.Merge(value) instead")]
        public static void Populate<T>(this JToken value, T target) where T : class
        {
            using (var sr = value.CreateReader())
            {
                Newtonsoft.Json.JsonSerializer.CreateDefault().Populate(sr, target); // Uses the system default JsonSerializerSettings
            }
        }

        /// <summary>
        /// Fastest engine to serialize object
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ToUtf8Json(this object src) => Utf8Json.JsonSerializer.ToJsonString(src);

        /// <summary>
        /// Newtonsoft engine to serialize object
        /// </summary>
        /// <param name="src"></param>
        /// <param name="ignoreNullValue">Ignore (exclude) null value property in the result json</param>
        /// <returns></returns>
        public static string ToNewtonsoftJson(this object src, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None, bool ignoreNullValue = false)
            => ignoreNullValue ? Newtonsoft.Json.JsonConvert.SerializeObject(src, formatting, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) :
            Newtonsoft.Json.JsonConvert.SerializeObject(src, formatting);

        /// <summary>
        /// Use dotnet core default <see cref="JsonSerializer"/> to <see cref="JsonSerializer.Serialize"/> <paramref name="obj"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="writeIndented"></param>
        /// <returns></returns>
        public static string ToJson(this object obj, bool writeIndented = false) => System.Text.Json.JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = writeIndented });
    }
}
