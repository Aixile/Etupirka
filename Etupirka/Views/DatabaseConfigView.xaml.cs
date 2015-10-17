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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Etupirka.Views
{
	/// <summary>
	/// Interaction logic for DisplayConfigView.xaml
	/// </summary>
	public partial class DatabaseConfigView : UserControl
	{

		public bool UseOfflineDatabase { get; set; }
		public string DatabaseSyncServer { get; set; }

		public DatabaseConfigView()
		{
			InitializeComponent();
			this.DataContext = this;
			UseOfflineDatabase = Properties.Settings.Default.useOfflineESDatabase;
			DatabaseSyncServer = Properties.Settings.Default.databaseSyncServer;
		}
	}
}
