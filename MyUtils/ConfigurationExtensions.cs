using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyUtils
{
    public static class ConfigurationExtensions
    {
        public static JToken Serialize(this IConfiguration config)
        {
            // Array-type handling
            var children = config.GetChildren();
            if (children.Any() && children.All(child => int.TryParse(child.Key, out _)))
            {
                JArray jArray = new JArray();
                foreach (var child in children.Where(x => x.Value != null))
                {
                    jArray.Add(Serialize(child));
                }
                return jArray;
            }

            // Normal handling
            JObject obj = new JObject();
            foreach (var child in children)
            {
                obj.Add(child.Key, Serialize(child));
            }

            if (!obj.HasValues && config is IConfigurationSection section)
                return section.Value is null ? null : new JValue(section.Value);

            return obj;
        }

        public static Dictionary<string, string> GetDictionary(this IConfiguration config, string configKey)
        {
            var result = new Dictionary<string, string>();
            config.GetSection(configKey).Bind(result);

            return result;
        }
    }
}
