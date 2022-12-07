using ModernStandbyTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;


namespace ModernStandbyFix
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private const string LogName = "Log.txt";
        private static List<string> SuspendedAdapters = new List<string>();

        protected override void OnStartup(StartupEventArgs e)
        {
            RestartAsAdmin(GetApplicationPath(), e);

            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }

            RegisterForPowerNotifications();
        }

        public static string GetApplicationPath()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            // it returns .dll, we need .exe
            return Path.ChangeExtension(assemblyPath, "exe");
        }

        public static void LogIntoFile(string message)
        {
            File.AppendAllText(Path.Combine(AppPath, LogName), $"[{DateTime.Now.Date} {DateTime.Now.TimeOfDay}]: {message}{Environment.NewLine}");
        }

        private void RegisterForPowerNotifications()
        {
            try
            {
                IntPtr registrationHandle = new IntPtr();
                DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS recipient = new DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS();
                recipient.Callback = new DeviceNotifyCallbackRoutine(DeviceNotifyCallback);
                recipient.Context = IntPtr.Zero;

                IntPtr pRecipient = Marshal.AllocHGlobal(Marshal.SizeOf(recipient));
                Marshal.StructureToPtr(recipient, pRecipient, false);

                uint result = PowerRegisterSuspendResumeNotification(DEVICE_NOTIFY_CALLBACK, ref recipient, ref registrationHandle);

                if (result != 0)
                    LogIntoFile("Error registering for power notifications: " + Marshal.GetLastWin32Error());
                else
                    LogIntoFile("Successfully Registered for power notifications!");
            }
            catch (Exception ex)
            {
                LogIntoFile(ex.Message);
            }
        }

        private static void RestartAsAdmin(string applicationPath, StartupEventArgs e)
        {

            // Check if the current process is running as an administrator
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                // Start the application as an administrator
                var startInfo = new ProcessStartInfo(applicationPath);
                startInfo.Verb = "runas"; // Run the application as an administrator
                startInfo.UseShellExecute = true;
                startInfo.Arguments = String.Join(' ', e.Args);
                Process.Start(startInfo);

                // Exit the current process
                Environment.Exit(0);
            }
        }

        private static int DeviceNotifyCallback(IntPtr context, int type, IntPtr setting)
        {
            LogIntoFile($"Device notify callback called: context: {context},type: {type}, setings: {setting}");

            switch (type)
            {
                case PBT_APMRESUMEAUTOMATIC:
                    ResumeAllSuspendedNetworkAdapters();
                    break;
                case PBT_APMSUSPEND:
                    SuspendAllEnabledNetworkAdapters();
                    break;
                case PBT_APMRESUMESUSPEND:
                case PBT_APMPOWERSTATUSCHANGE:
                case PBT_POWERSETTINGCHANGE:
                default:
                    break;
            }

            return 0;
        }

        private static void SuspendAllEnabledNetworkAdapters()
        {
            App.LogIntoFile("Suspending enabled network adapters");
            foreach (var adapter in NetworkAdaptersUtils.NetworkAdapters())
            {
                if (adapter.isEnabled)
                {
                    NetworkAdaptersUtils.DisableAdapterAsync(adapter.name);
                    SuspendedAdapters.Add(adapter.name);
                }
            }
        }

        private static void ResumeAllSuspendedNetworkAdapters()
        {
            App.LogIntoFile("Resuming disabled adapters");
            foreach (var adapter in SuspendedAdapters)
            {
                NetworkAdaptersUtils.EnableAdapterAsync(adapter);
            }

            SuspendedAdapters.Clear();
        }

        [DllImport("Powrprof.dll", SetLastError = true)]
        static extern uint PowerRegisterSuspendResumeNotification(uint flags, ref DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS receipient, ref IntPtr registrationHandle);


        private const int WM_POWERBROADCAST = 536; // (0x218)
        private const int PBT_APMPOWERSTATUSCHANGE = 10; // (0xA) - Power status has changed.
        private const int PBT_APMRESUMEAUTOMATIC = 18; // (0x12) - Operation is resuming automatically from a low-power state.This message is sent every time the system resumes.
        private const int PBT_APMRESUMESUSPEND = 7; // (0x7) - Operation is resuming from a low-power state.This message is sent after PBT_APMRESUMEAUTOMATIC if the resume is triggered by user input, such as pressing a key.
        private const int PBT_APMSUSPEND = 4; // (0x4) - System is suspending operation.
        private const int PBT_POWERSETTINGCHANGE = 32787; // (0x8013) - A power setting change event has been received.
        private const int DEVICE_NOTIFY_CALLBACK = 2;

        /// <summary>
        /// OS callback delegate definition
        /// </summary>
        /// <param name="context">The context for the callback</param>
        /// <param name="type">The type of the callback...for power notifcation it's a PBT_ message</param>
        /// <param name="setting">A structure related to the notification, depends on type parameter</param>
        /// <returns></returns>
        delegate int DeviceNotifyCallbackRoutine(IntPtr context, int type, IntPtr setting);

        /// <summary>
        /// A callback definition
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS
        {
            public DeviceNotifyCallbackRoutine Callback;
            public IntPtr Context;
        }
    }
}
