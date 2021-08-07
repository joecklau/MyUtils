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
        /// Fastest engine to serialize object
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ToUtf8Json(this object src) => Utf8Json.JsonSerializer.ToJsonString(src);

        /// <summary>
        /// Use dotnet core default <see cref="JsonSerializer"/> to <see cref="JsonSerializer.Serialize"/> <paramref name="obj"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj) => JsonSerializer.Serialize(obj);
    }
}
