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
using System.Reflection;

namespace Etupirka.Views
{
	public class EnumToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value.Equals(true) ? parameter : Binding.DoNothing;
		}
	}
	public class InverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(bool))
				throw new InvalidOperationException("The target must be a boolean");

			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public enum ProxyType
	{
		None=0, Http=1, Socks4=2, Socks5=3
	}
	/// <summary>
	/// Interaction logic for NetworkConfigView.xaml
	/// </summary>
	public partial class NetworkConfigView : UserControl
	{
		public ProxyType MyProxyType { get; set; }
		public string ProxyAddress { get; set; }
		public int ProxyPort { get; set; }
		public string ProxyUser { get; set; }

		public NetworkConfigView()
		{
			InitializeComponent();
			this.DataContext = this;
			loadSetting();
		}
		private void loadSetting()
		{
			int proxyType=(int)Properties.Settings.Default["proxyType"];
			MyProxyType = (ProxyType)proxyType;
			ProxyAddress = (string)Properties.Settings.Default["proxyAddress"];
			ProxyPort = (int)Properties.Settings.Default["proxyPort"];
			ProxyUser = (string)Properties.Settings.Default["proxyUser"];
			passwordBox.Password = (string)Properties.Settings.Default["proxyPassword"];
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Console.Write(MyProxyType);

		}



	}
}
