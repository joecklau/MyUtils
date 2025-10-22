using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace MyUtils.SystemMemory
{
    /// <summary>
    /// Reference: https://gunnarpeipman.com/dotnet-core-system-memory/
    /// </summary>
    public class MemoryMetricsClient
    {
        // Cached docker environment detection so we only evaluate once per process lifetime
        private static readonly bool _isDocker = DetectDocker();
        public static bool IsDocker => _isDocker;

        public MemoryMetrics GetMetrics()
        {
            // In dockerized environment we skip executing external commands and just return empty metrics
            if (IsDocker)
            {
                return new MemoryMetrics();
            }

            if (IsUnix())
            {
                return GetUnixMetrics();
            }

            return GetWindowsMetrics();
        }

        private bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        // Performs the actual detection logic (executed once, cached in _isDocker)
        private static bool DetectDocker()
        {
            try
            {
                var dotnetInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
                if (string.Equals(dotnetInContainer, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ECS_CONTAINER_METADATA_URI_V4")))
                {
                    return true;
                }

                if (File.Exists("/.dockerenv"))
                {
                    return true;
                }

                if (File.Exists("/proc/1/cgroup"))
                {
                    var cgroup = File.ReadAllText("/proc/1/cgroup");
                    if (cgroup.Contains("docker") || cgroup.Contains("kubepods") || cgroup.Contains("containerd"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore detection errors, default to false
            }
            return false;
        }

        private MemoryMetrics GetWindowsMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split('\n');
            var freeMemoryParts = lines[0].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
            metrics.Used = metrics.Total - metrics.Free;

            return metrics;
        }

        private MemoryMetrics GetUnixMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                //Console.WriteLine(output);
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                return new MemoryMetrics();
            }

            var lines = output.Split(new[] { '\n' });
            var memory = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = double.Parse(memory[1]);
            metrics.Used = double.Parse(memory[2]);
            metrics.Free = double.Parse(memory[3]);

            return metrics;
        }
    }
}
