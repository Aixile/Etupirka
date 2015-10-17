using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Etupirka.Control
{
	/// <summary>
	/// Interaction logic for TimeControl.xaml
	/// </summary>
	public partial class TimeControl : UserControl, INotifyPropertyChanged
	{
		public TimeControl()
		{
			InitializeComponent();
			this.DataContext = this;
		}
		public int Value
		{
			get { return _hour*3600+_min*60+_sec; }
			set { 
				Hours = value / 3600;
				Minutes = (value / 60) % 60;
				Seconds = value % 60;
			}
		}

		int _hour;
		public int Hours
		{
			get { return _hour; }
			set {  _hour=value;
			OnPropertyChanged("Hours");
			}
		}
		int _min;
		public int Minutes
		{
			get { return _min; }
			set { _min=value;
			OnPropertyChanged("Minutes");
			}
		}
		int _sec;
		public int Seconds
		{
			get { return _sec; }
			set { _sec=value;
			OnPropertyChanged("Seconds");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged == null) return;
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	} 

}
