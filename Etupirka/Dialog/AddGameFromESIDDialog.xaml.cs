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

namespace Etupirka.Dialogs
{
	/// <summary>
	/// Interaction logic for addGameFromESID.xaml
	/// </summary>
	public partial class AddGameFromESIDDialog : MetroWindow
	{
		public AddGameFromESIDDialog()
		{
			InitializeComponent();
			if (Properties.Settings.Default.disableGlowBrush)
			{
				this.GlowBrush = null;
			}
		}

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		public int Value
		{
			get
			{
				return (int)ESID.Value;
			}
		}
	}
}
