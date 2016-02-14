using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using System.Windows.Navigation;

namespace Etupirka.Dialog
{
    /// <summary>
    /// DisplaySettingsHelpDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DisplaySettingsHelpDialog : MetroWindow
    {
        public DisplaySettingsHelpDialog()
        {
            InitializeComponent();
        }
		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
    }
}
