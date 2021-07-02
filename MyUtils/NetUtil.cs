using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace MyUtils
{
    public static class NetUtil
    {
        /// <summary>
        /// Check if a Local TCP port is available
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool CheckLocalTcpPortAvailable(int port)
        {
            //int port = 456; //<--- This is your value
            //bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    //isAvailable = false;
                    //break;
                    return false;
                }
            }

            return true;
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
        public static int GetAvailableLocalTcpPort(int minValue = 49152, int maxValue = 65535, int maxTrialLimit = 30)
        {
            if (minValue < 0 || maxValue > 65535)
            {
                throw new ArgumentOutOfRangeException("A valid TCP port should be within 0 to 65535");
            }

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
            } while (!CheckLocalTcpPortAvailable(port));

            return port;
        }
    }
}
