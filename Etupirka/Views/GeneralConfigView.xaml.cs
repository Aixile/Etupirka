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
	public partial class GeneralConfigView : UserControl
	{
		public bool DifferExecuatableGame { get; set; }
		public bool SetStartUp { get; set; }
		public bool WatchProcess { get; set; }
		public bool MinimizeAtStartup { get; set; }
		public int MontiorInterval { get; set; }
		public bool PlayVoice { get; set; }
		public bool AskBeforeExit { get; set; }
		public bool CheckUpdate { get; set; }
		public bool DisableGlowBrush { get; set; }
        public bool HideListWhenPlaying { get; set; }

        public GeneralConfigView()
		{
			InitializeComponent();
			this.DataContext = this;
			DifferExecuatableGame = Properties.Settings.Default.differExecuatableGame;
			SetStartUp = Properties.Settings.Default.setStartUp;
			AskBeforeExit = Properties.Settings.Default.askBeforeExit;
			WatchProcess = Properties.Settings.Default.watchProcess;
			MontiorInterval = Properties.Settings.Default.monitorInterval;
			PlayVoice = Properties.Settings.Default.playVoice;
			MinimizeAtStartup = Properties.Settings.Default.minimizeAtStartup;
			CheckUpdate = Properties.Settings.Default.checkUpdate;
			DisableGlowBrush = Properties.Settings.Default.disableGlowBrush;
            HideListWhenPlaying = Properties.Settings.Default.hideListWhenPlaying;
		}
	}
}
