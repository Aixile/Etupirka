using System;
using System.Diagnostics;
using System.IO;

namespace SatoruErogeTimer
{
	[Serializable]
	public class ErogeNode : Eroge
	{
		string path;
		public string Path
		{
			get { return path; }
			set { path = value; }
		}

		public string processName {get;set;}

		public enum RunningStatus{Focused,Resting,Unfocused};
		private RunningStatus status;
		public RunningStatus Status
		{
			get { return status; }
			set { status = value; }
		}

		public ErogeNode() { }
		public ErogeNode(string _title, int _time, string _addr,RunningStatus _status = RunningStatus.Resting)
		{
			title = _title;
			time = _time;
			path = _addr;
		}
		public string getState()
		{
			return status.ToString();
		}
		public bool run()
		{
			ProcessStartInfo start = new ProcessStartInfo(path);
			start.CreateNoWindow = false;
			start.RedirectStandardOutput = true;
			start.RedirectStandardInput = true;
			start.UseShellExecute = false;
			start.WorkingDirectory = new FileInfo(path).DirectoryName;
			try
			{
				Process p = Process.Start(start);
			}
			catch
			{
				return false;
			}
			return true;
		}
	}
}
