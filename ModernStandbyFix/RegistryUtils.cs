using Microsoft.Win32;
using System;

namespace ModernStandbyFix
{
    internal class RegistryUtils
    {
        public static bool IsRunningOnStartup(string applicationPath)
        {
            try
            {
                // Check if the application is already in the startup folder.
                var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                var value = key?.GetValue("ModernStandbyFix");
                return value != null && ((string)value).StartsWith(applicationPath);
            }
            catch (Exception ex)
            {
                App.LogIntoFile(ex.Message);
                return false;
            }
        }

        public static bool ToggleRunOnStartup(string applicationPath)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (IsRunningOnStartup(applicationPath))
                {
                    key?.DeleteValue("ModernStandbyFix");
                }
                else
                {
                    key?.SetValue("ModernStandbyFix", applicationPath + " /minimized");
                }
            }
            catch (Exception ex)
            {
                App.LogIntoFile(ex.Message);
                return false;
            }
            return true;
        }
    }
}
