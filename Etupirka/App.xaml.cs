using Etupirka.Properties;
using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
namespace Etupirka
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application,Microsoft.Shell.ISingleInstanceApp
	{
		private const string Unique = "15ea85f4e7c301a460d4fc862c9895ca4ff1dffe";

		[STAThread]
		public static void Main()
		{
			if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
			{
				string [] args=Environment.GetCommandLineArgs();
				if (args.Count() > 1&&args[1].Equals("-min"))
				{
					Settings.Default.Do_minimize = true;

				}
				var application = new App();
				application.InitializeComponent();

                //Load lang
                I18n.LoadLanguage();

                application.Run();


				// Allow single instance code to perform cleanup operations
				SingleInstance<App>.Cleanup();
			}
		}

		#region ISingleInstanceApp Members
		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			// Handle command line arguments of second instance
			return true;
		}
		#endregion

	

	}

}
