using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernStandbyFix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            versionTextBlock.Text = "Version: " + Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();
            runOnStartupCheckBox.IsChecked = RegistryUtils.IsRunningOnStartup(App.GetApplicationPath());
            runOnStartupCheckBox.Checked += runOnStartupCheckBox_Checked;
            runOnStartupCheckBox.Unchecked += runOnStartupCheckBox_Checked;
        }

        private void runOnStartupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (RegistryUtils.ToggleRunOnStartup(App.GetApplicationPath()))
            {
                runOnStartupCheckBox.IsChecked = RegistryUtils.IsRunningOnStartup(App.GetApplicationPath());
            }
        }

        private void hideButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
