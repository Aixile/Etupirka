using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace Etupirka.Dialog
{
    /// <summary>
    /// DisplaySettingsDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DisplaySettingsDialog : MetroWindow
    {
        public DisplayInfo Result;

        public DisplaySettingsDialog(DisplayInfo displayInfo)
        {
            InitializeComponent();
			if (Properties.Settings.Default.disableGlowBrush)
			{
				this.GlowBrush = null;
			}
            Result = getCurrentDisplayInfo(displayInfo);
            DeviceListView.ItemsSource = Result.devices;
        }

        private DisplayInfo getCurrentDisplayInfo(DisplayInfo displayInfo)
        {
            DisplayInfo res = DisplaySettings.GetDisplayDevices();
            foreach (var device in res.devices)
            {
                var device2 = displayInfo.devices.FirstOrDefault(_device => _device.DeviceID == device.DeviceID && _device.Enabled == true);
                if (device2 != null)
                {
                    device.Enabled = true;
                    device.Scaling = device2.Scaling;
                }
            }
            return res;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var dsh=new DisplaySettingsHelpDialog();
			dsh.Owner = this;
			dsh.ShowDialog();
        }
    }
}
