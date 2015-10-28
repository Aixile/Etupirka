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
		public static readonly DependencyProperty ValuePropert =
			DependencyProperty.Register("Value", typeof(int), typeof(TimeControl),
			new PropertyMetadata(0,new PropertyChangedCallback(OnValueChanged)));
		
		
		public int Value
		{
			get
			{
				return /*_hour*3600+_min*60+_sec; */
					(int)GetValue(ValuePropert);
			}
			set { 
				Hours = value / 3600;
				Minutes = (value / 60) % 60;
				Seconds = value % 60;
				SetValue(ValuePropert, value);
			}
		}
		private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
		//	System.Console.WriteLine(e.ToString());
			TimeControl ctrl = obj as TimeControl;
			if (ctrl != null)
			{
				ctrl.Value =(int) e.NewValue;
			}
		}

		int _hour;
		public int Hours
		{
			get { return _hour; }
			set {  _hour=value;
			OnPropertyChanged("Hours");
			SetValue(ValuePropert, Hours * 3600 + Minutes * 60 + Seconds);
			}
		}
		int _min;
		public int Minutes
		{
			get { return _min; }
			set { _min=value;
			OnPropertyChanged("Minutes");
			SetValue(ValuePropert, Hours * 3600 + Minutes * 60 + Seconds);
			}
		}
		int _sec;
		public int Seconds
		{
			get { return _sec; }
			set { _sec=value;
			OnPropertyChanged("Seconds");
			SetValue(ValuePropert, Hours * 3600 + Minutes * 60 + Seconds);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged == null) return;
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}



		private void ss_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (Seconds < 10||ss.Value==null)
			{
				e.Handled = false;
			}
			else
			{
				e.Handled = true;
			}

		}

		private void mm_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (Minutes < 10 || mm.Value == null)
			{
				e.Handled = false;
			}
			else
			{
				e.Handled = true;
			}
		}

		private void hh_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (Hours < 1000 || hh.Value == null)
			{
				e.Handled = false;
			}
			else
			{
				e.Handled = true;
			}
		}
	} 

}
