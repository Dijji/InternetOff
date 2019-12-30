using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Management.Automation;
using System.Threading.Tasks;

namespace InternetOff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string RuleName = "Block Internet";
        private State s = new State();

        class State : INotifyPropertyChanged
        {
            private bool? isConnected = null;
            private bool isWorking;

            public bool CanTurnOn { get { return IsConnected == false && !IsWorking; } }
            public bool CanTurnOff { get { return IsConnected == true && !IsWorking; } }
            public Brush StopGoColour
            {
                get
                {
                    if (IsConnected == null || IsWorking)
                        return Brushes.CornflowerBlue;
                    else
                        return (bool)IsConnected ? Brushes.LightGreen : Brushes.OrangeRed;
                }
            }

            public bool? IsConnected 
            { 
                get { return isConnected; }
                set { isConnected = value; OnPropertyChanged(nameof(StopGoColour));
                        OnPropertyChanged(nameof(CanTurnOn)); OnPropertyChanged(nameof(CanTurnOff)); }
            }

            public bool IsWorking
            {
                get { return isWorking; }
                set { isWorking = value; OnPropertyChanged(nameof(IsWorking)); OnPropertyChanged(nameof(StopGoColour));
                    OnPropertyChanged(nameof(CanTurnOn)); OnPropertyChanged(nameof(CanTurnOff)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(info));
                    });
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = s;
            Task.Run(() =>
            {
                s.IsConnected = !IsBlocked();
            });
        }
        
        private void cbOff_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                AddBlock();
            });
        }

        private void cbOn_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                RemoveBlock();
            });
        }

        private void AddBlock()
        {
            string error = null;
            s.IsWorking = true;

            try
            {
                var script = new StringBuilder("New-NetFirewallRule ");
                script.AppendFormat("-Name \"{0}\" ", RuleName);
                script.Append("-DisplayName \"Block Internet access\" ");
                script.Append("-Description \"Used to block all internet access\" ");
                script.Append("-Enabled True ");
                script.Append("-Direction Outbound ");
                script.Append("-Action Block ");
                script.Append("-InterfaceType Any ");
                script.Append("-RemoteAddress Internet");

                var powerShell = PowerShell.Create().AddScript(script.ToString());

                var result = powerShell.Invoke();

                if (result.Count == 0)
                    error = "Powershell rejected command";
                else if (result[0].Properties["DisplayName"] == null)
                    error = result[0].ToString();
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
            }

            s.IsWorking = false;
            if (error != null)
                MessageBox.Show(String.Format("Firewall operation failed with: {0}", error), "Error");
            else
                s.IsConnected = false;
        }

        private void RemoveBlock()
        {
            string error = null;
            s.IsWorking = true;
            try
            {
                var script = new StringBuilder("Remove-NetFirewallRule ");
                script.AppendFormat("-Name \"{0}\" ", RuleName);
                var powerShell = PowerShell.Create().AddScript(script.ToString());

                var result = powerShell.Invoke();

                if (result.Count > 0)
                    error = result[0].ToString();
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
            }

            s.IsWorking = false;
            if (error != null)
                MessageBox.Show(String.Format("Firewall operation failed with: {0}", error), "Error");
            else
                s.IsConnected = true;
        }

        private bool IsBlocked()
        {
            bool isBlocked = false;
            string error = null;
            s.IsWorking = true;
            try
            {
                var script = new StringBuilder("Get-NetFirewallRule ");
                script.AppendFormat("-Name \"{0}\" ", RuleName);
                var powerShell = PowerShell.Create().AddScript(script.ToString());
                var result = powerShell.Invoke();
                isBlocked = result.Count() > 0;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
            }
            s.IsWorking = false;
            if (error != null)
                MessageBox.Show(String.Format("Firewall operation failed with: {0}", error), "Error");
            return isBlocked;
        }
    }
}
