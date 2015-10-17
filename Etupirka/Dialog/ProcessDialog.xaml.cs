using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace Etupirka.Dialog
{
	/// <summary>
	/// Interaction logic for ProcessDialog.xaml
	/// </summary>
	public class ProcDate
	{
		public ProcDate(Process p)
		{
			ProcName = Convert.ToString(p.ProcessName);
			ProcPid = p.Id;
			try
			{
				ProcPath = Convert.ToString(p.MainModule.FileName);
				ProcName = ProcName + System.IO.Path.GetExtension(ProcPath);
				ProcTitle = Convert.ToString(p.MainWindowTitle);
			}
			catch
			{
			}
		}
		public string ProcName { get; set; }
		public string ProcPath { get; set; }
		public string ProcTitle { get; set; }
		public int ProcPid { get; set; }
	}

	public partial class ProcessDialog : MetroWindow
	{
		private ObservableCollection<ProcDate> items;
		public ProcessDialog()
		{
			InitializeComponent();
			items = new ObservableCollection<ProcDate>();
			ProcListView.ItemsSource = items;
			getProcList();
		}
		void getProcList()
		{
			Process[] proc = Process.GetProcesses();
			foreach (Process p in proc)
			{
				items.Add(new ProcDate(p));
			}
		}

		public ProcDate SelectedProc { get; set; }
		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			ProcListView.UnselectAll();
			items.Clear();
			getProcList();
		}

		private void ProcListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var listView = (ListView)sender;
			var item = listView.ContainerFromElement((DependencyObject)e.OriginalSource) as ListViewItem;
			if (item != null)
			{
				int index = this.ProcListView.SelectedIndex;
				SelectedProc = (ProcDate)this.ProcListView.SelectedItem;
				this.DialogResult = true;
			}
		}

		private void MetroWindow_Deactivated(object sender, EventArgs e)
		{
			ProcListView.UnselectAll();
		}
	}
}
