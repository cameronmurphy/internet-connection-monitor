using Camurphy.InternetConnectionMonitor.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Camurphy.InternetConnectionMonitor
{
    class Program
    {
        private const int WebClientTimeoutMilliseconds = 60 * 1000;

        static void Main(string[] args)
        {
            var connectivityResponse = TestConnectivity();
            bool online = connectivityResponse.Online;

            // Treat being outside of a VPN as offline
            if (online && connectivityResponse.PublicIpAddress.Equals(Settings.Default.UnprotectedPublicIpAddress)) {
                online = false;
            }

            if (online)
            {
                PerformuTorrentProcessAction(ProcessAction.StartIfNotOpen);
            }
            else
            {
                PerformuTorrentProcessAction(ProcessAction.Stop);
                RunViscosityCommand(ViscosityCommand.DisconnectAll);
                RunViscosityCommand(ViscosityCommand.Connect);
            }
        }

        private static ConnectivityTestResponse TestConnectivity()
        {
            ConnectivityTestResponse response = new ConnectivityTestResponse();

            using (var client = new AdvancedWebClient())
            {
                client.Timeout = WebClientTimeoutMilliseconds;

                try
                {
                    response.PublicIpAddress = client.DownloadString(Settings.Default.PublicIpAddressUrl).Trim();
                    response.Online = true;
                }
                catch (WebException ex)
                {
                    // Presume all WebExceptions are failures to connect
                    response.Online = false;
                    response.WebException = ex;
                }
            }

            return response;
        }

        private static void PerformuTorrentProcessAction(ProcessAction action)
        {
            Process[] processes = Process.GetProcessesByName(Settings.Default.uTorrentProcessName);

            switch (action)
            {
                case ProcessAction.StartIfNotOpen:
                    if (!processes.Any())
                    {
                        ProcessStartInfo processStartInfo = new ProcessStartInfo();
                        processStartInfo.FileName = Settings.Default.uTorrentExecutablePath;
                        processStartInfo.WindowStyle = ProcessWindowStyle.Maximized;

                        Process.Start(processStartInfo);
                    }
                    break;
                case ProcessAction.Stop:
                    foreach (Process process in processes)
                    {
                        process.CloseMainWindow();
                    }
                    break;
            }
        }

        private static void RunViscosityCommand(ViscosityCommand command)
        {
            Process process = null;

            switch (command)
            {
                case ViscosityCommand.DisconnectAll:
                    process = Process.Start(Settings.Default.ViscosityExecutablePath, "disconnect all");
                    break;
                case ViscosityCommand.Connect:
                    process = Process.Start(Settings.Default.ViscosityExecutablePath, String.Format("connect \"{0}\"", Settings.Default.ViscosityConnectionName));
                    break;
            }
        }
    }
}