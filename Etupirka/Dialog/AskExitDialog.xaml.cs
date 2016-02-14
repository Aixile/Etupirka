using MahApps.Metro.Controls;
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

namespace Etupirka.Dialog
{
	/// <summary>
	/// Interaction logic for AskExitDialog.xaml
	/// </summary>
	public partial class AskExitDialog : MetroWindow
	{
		public AskExitDialog()
		{
			InitializeComponent();
			if (Properties.Settings.Default.disableGlowBrush)
			{
				this.GlowBrush = null;
			}
			this.DataContext = this;
		}
		public bool DoNotDisplay { get; set; }

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{

			this.DialogResult = true;
		}


	}
}
