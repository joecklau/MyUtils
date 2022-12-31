using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyUtils
{
    /// <summary>
    /// Adjusted version basing on https://learn.microsoft.com/en-us/answers/questions/609232/how-to-save-the-updates-i-made-to-appsettings-conf.html
    /// with IWebHostEnvironment replaced by FileInfo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWritableOptions<out T> : IOptions<T> where T : class, new()
    {
        void Update(Action<T> applyChanges);
    }

    /// <summary>
    /// Adjusted version basing on https://learn.microsoft.com/en-us/answers/questions/609232/how-to-save-the-updates-i-made-to-appsettings-conf.html
    /// with IWebHostEnvironment replaced by FileInfo. Also accept null/empty _section (i.e. target section is root of configuration file)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IOptionsMonitor<T> _options;
        private readonly IConfigurationRoot _configuration;
        private readonly string _section;
        private readonly FileInfo _fileInfo;
        private readonly object fileLock = new object();

        public WritableOptions(
            IOptionsMonitor<T> options,
            IConfigurationRoot configuration,
            string section,
            FileInfo fileInfo)
        {
            _options = options;
            _configuration = configuration;
            _section = section;
            _fileInfo = fileInfo;
        }
        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);
        public void Update(Action<T> applyChanges)
        {
            lock (fileLock)
            {
                var physicalPath = _fileInfo.FullName;
                var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(physicalPath));
                var sectionObject = jObject.TryGetValue(_section, out JToken section) ?
                    JsonConvert.DeserializeObject<T>(section.ToString()) : (Value ?? new T());
                applyChanges(sectionObject);
                if (string.IsNullOrWhiteSpace(_section))
                {
                    File.WriteAllText(physicalPath, JsonConvert.SerializeObject(sectionObject, Formatting.Indented));
                }
                else
                {
                    jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
                    File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
                }
                _configuration.Reload();
            }
        }
    }
}
