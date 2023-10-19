using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MyUtils
{
    public static class NetUtil
    {
        /// <summary>
        /// Refer to SystemBrowser.
        /// This handling can avoid error across different OS
        /// </summary>
        /// <param name="url"></param>
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Windows will reserve some ports even they are not used yet. See https://superuser.com/questions/1486417/unable-to-start-kestrel-getting-an-attempt-was-made-to-access-a-socket-in-a-way
        /// </summary>
        /// <returns></returns>
        public static List<(int start, int end)> GetWindowsReservedPortRanges()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return new List<(int start, int end)>();
            }

            // Start the child process.
            Process p = new Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "netsh";
            p.StartInfo.Arguments = "interface ipv4 show excludedportrange protocol=tcp";
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var portRanges = lines.Select(text => Regex.Match(text, "^ *([0-9]+) +([0-9]+)"))
                .Where(x => x.Success)
                .Select(x => (int.Parse(x.Groups[1].Value), int.Parse(x.Groups[2].Value)))
                .ToList();

            return portRanges;
        }

        /// <summary>
        /// Check if a Local TCP port is available
        /// </summary>
        /// <param name="port"></param>
        /// <param name="scanWindowsReservedPorts">
        /// Invoke <see cref="GetWindowsReservedPortRanges"/> before check availability. 
        /// If you're going to call this method on a list of ports, you should consider pre-filtering the ports with <see cref="GetWindowsReservedPortRanges"/> rather than using this as this will repeat call the method</param>
        /// <returns></returns>
        public static bool CheckLocalTcpPortAvailable(int port, bool scanWindowsReservedPorts = false)
        {
            if (scanWindowsReservedPorts)
            {
                var windowsReservedPortRanges = GetWindowsReservedPortRanges();
                if (windowsReservedPortRanges.Any(x => port >= x.start && port <= x.end))
                {
                    return false;
                }
            }

            //int port = 456; //<--- This is your value
            //bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            // Joe's normal usage: Not just Active TCP connections, but also the current port listeners should also be counted
            int[] connectedOrOccupiedPorts = ipGlobalProperties.GetActiveTcpConnections().Select(x => x.LocalEndPoint)
                .Concat(ipGlobalProperties.GetActiveTcpListeners())
                .Concat(ipGlobalProperties.GetActiveUdpListeners())
                .Select(x => x.Port)
                .Distinct()
                .OrderBy()
                .ToArray();
            
            return !connectedOrOccupiedPorts.Contains(port);

            // Original version from https://stackoverflow.com/a/570461/4684232 which only check CONNECTED ports, but not LISTENING ports (e.g. 80, 443 ports of web server)
            //TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            //foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            //{
            //    if (tcpi.LocalEndPoint.Port == port)
            //    {
            //        //isAvailable = false;
            //        //break;
            //        return false;
            //    }
            //}

            //return true;
        }

        /// <summary>
        /// Same as <see cref="GetAvailableLocalTcpPort"/>
        /// </summary>
        /// <returns></returns>
        // ref http://stackoverflow.com/a/3978040
        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        /// <summary>
        /// Get a available port no
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int GetAvailableLocalTcpPort(int minValue = 49152, int maxValue = 65535, int maxTrialLimit = 30, bool scanWindowsReservedPorts = false)
        {
            if (minValue < 0 || maxValue > 65535)
            {
                throw new ArgumentOutOfRangeException("A valid TCP port should be within 0 to 65535");
            }

            var windowsReservedPortRanges = scanWindowsReservedPorts ? GetWindowsReservedPortRanges() : new List<(int start, int end)>();

            var rand = new Random();
            int trialCount = 0;
            int port;
            do
            {
                port = rand.Next(minValue, maxValue);
                trialCount++;
                if (trialCount > maxTrialLimit)
                {
                    throw new Exception($"After {trialCount} trials, there is still no available port within {minValue} and {maxValue}. Stop trying.");
                }
            } while (windowsReservedPortRanges.Any(x => port >= x.start && port <= x.end) || !CheckLocalTcpPortAvailable(port, scanWindowsReservedPorts: false));

            return port;
        }
    }
}
