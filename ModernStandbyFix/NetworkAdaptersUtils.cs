using ModernStandbyFix;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModernStandbyTest
{
    public static class NetworkAdaptersUtils
    {
        // returns list of adapters - as a tuple - name, isUp
        public static List<(string name, bool isEnabled)> NetworkAdapters()
        {
            var adapters = new List<(string name, bool isEnabled)>();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                
                adapters.Add((nic.Name, nic.OperationalStatus == OperationalStatus.Up));
            }
            return adapters;
        }

        private static void ExecuteWaitProcess(string cmd, string args)
        {

            var psi = new ProcessStartInfo(cmd, args);
            var p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }

        public static Task<bool> EnableAdapterAsync(string interfaceName, int timeOut = 2000)
        {
            bool enabled = false;
            int timeElapsed = 0;
            int timeWait = 100;
            var locker = new object();

            return Task.Run(() =>
            {
                ExecuteWaitProcess("netsh", "interface set interface \"" + interfaceName + "\" enable");

                do
                {
                    lock (locker) enabled = IsAdpapterEnabled(interfaceName);
                    Thread.Sleep(timeWait);
                    lock (locker) timeElapsed += timeWait;

                } while (!enabled && timeElapsed < timeOut);
                App.LogIntoFile("Enabled adapter: " + interfaceName);
                return enabled;
            });
        }

        public static Task<bool> DisableAdapterAsync(string interfaceName, int timeOut = 2000)
        {
            bool disabled = false;
            int timeElapsed = 0;
            int timeWait = 100;
            var locker = new object();


            return Task.Run(() =>
            {
                ExecuteWaitProcess("netsh", "interface set interface \"" + interfaceName + "\" disable");

                do
                {
                    lock (locker) disabled =!IsAdpapterEnabled(interfaceName);
                    Thread.Sleep(timeWait);
                    lock (locker) timeElapsed += timeWait;

                } while (!disabled && timeElapsed < timeOut);
                App.LogIntoFile("Disabled adapter: " + interfaceName);
                return disabled;
            });
        }


        /// https://stackoverflow.com/a/314322/19305596
        public static bool IsAdpapterEnabled(string interfaceName)
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var selectedInterface = networkInterfaces.FirstOrDefault(n => n.Name == interfaceName);
            if (selectedInterface == null)
            {
                return false;
            }
            return selectedInterface.OperationalStatus == OperationalStatus.Up;
        }
    }
}
