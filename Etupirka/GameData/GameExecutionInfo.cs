﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Etupirka
{
	public enum ProcStat
	{
		NotExist=0,Rest=1,Unfocused=2,Focused=3
	}
	public class GameExecutionInfo : GameTime
	{

		public GameExecutionInfo()
		{
			isProcNEqExec = false;
            displayInfo = new DisplayInfo();
		}
		public GameExecutionInfo(string _title, string _path)
		{
			Title = _title;
			isProcNEqExec = false;
			ExecPath = _path;
			ProcPath = _path;
			GenerateUID();
            displayInfo = new DisplayInfo();
        }
		public GameExecutionInfo(int ESID)
		{
			erogameScapeID = ESID;
			updateInfoFromES();

			totalPlayTime = 0;
			firstPlayTime = new DateTime(0);
			lastPlayTime = new DateTime(0);

			isProcNEqExec = false;

			GenerateUID();

            displayInfo = new DisplayInfo();
        }

		public GameExecutionInfo(string _uid,string _title,string _brand,int _esid,DateTime _saleday,
			int _playtime,DateTime _firstplay,DateTime _lastplay,
			bool _pne,string _procpath,string _execpath, DisplayInfo _displayInfo)
		{
			uid = _uid;
			title = _title;
			brand = _brand;
			erogameScapeID = _esid;
			saleDay = _saleday;

			totalPlayTime = _playtime;
			firstPlayTime = _firstplay;
			lastPlayTime = _lastplay;

			isProcNEqExec = _pne;
			procPath = _procpath;
			execPath = _execpath;

            displayInfo = _displayInfo;
		}

		bool isPathExist;
		public bool IsPathExist
		{
			get
			{
				return isPathExist;
			}
			set
			{
				if (isPathExist != value)
				{
					isPathExist = value;
					OnPropertyChanged("IsPathExist");
				}
				if (!isPathExist)
				{
					Status = ProcStat.NotExist;
				}
			}
		}
		public bool CheckPath()
		{
			return System.IO.File.Exists(ExecPath);
		}

		protected ProcStat status;
		public ProcStat Status
		{
			get { return status; }
			set
			{
				if (status != value)
				{
					status = value;
					OnPropertyChanged("Status");
				}
			}
		}

		public bool UpdateStatus2(Dictionary<String, bool> dic,ref bool running,int time=0)
		{
			ProcStat LastStatus = Status;
			IsPathExist=CheckPath();
			if (!IsPathExist)
			{
				Status = ProcStat.NotExist;
			}
			else
			{
				Status = ProcStat.Rest;
				if (dic.ContainsKey(ProcPath.ToLower()))
				{
					running = true;
					if (FirstPlayTime.Ticks == 0)
					{
						FirstPlayTime = DateTime.Now;
					}
					if (dic[ProcPath.ToLower()])
					{
						Status = ProcStat.Focused;
						if (time != 0)
						{
							addTime(time);
						}
						return true;
					}
					else
					{
						Status = ProcStat.Unfocused;
					}

				}
			}
			if (LastStatus == ProcStat.Focused || LastStatus == ProcStat.Unfocused)
			{
				if (Status == ProcStat.Rest || Status == ProcStat.NotExist)
				{
					LastPlayTime = DateTime.Now;
                    DisplaySettings.RestoreDisplay(this);
					return true;
				}
			}
			return false;
		}

		protected bool isProcNEqExec;
		public bool IsProcNEqExec
		{
			get { return isProcNEqExec; }
			set { 
				isProcNEqExec = value;
				OnPropertyChanged("IsProcNEqExec");
			}
		}


		protected string procPath;
		public string ProcPath
		{
			get { return procPath; }
			set { 
				procPath = value;
				OnPropertyChanged("ProcPath");
				if (!isProcNEqExec)
				{
					OnPropertyChanged("ExecPath");
				}
				IsPathExist=CheckPath();
			}
		}

		protected string execPath;
		public string ExecPath
		{
			get { if (isProcNEqExec) return execPath; else return procPath; }
			set { 
				execPath = value;
				OnPropertyChanged("ExecPath");
			}
		}

		[XmlIgnore]
		protected DisplayInfo displayInfo;
        public DisplayInfo DisplayInfo
        {
            get
            {
                return displayInfo;
            }

            set
            {
                displayInfo = value;
            }
        }

        public Object Clone()
		{
			return MemberwiseClone();
		}

		public void Set(GameExecutionInfo t)
		{
			Title = t.Title;
			Brand = t.Brand;
			SaleDay = t.SaleDay;
			ErogameScapeID = t.ErogameScapeID;

			TotalPlayTime = t.TotalPlayTime;
			FirstPlayTime = t.FirstPlayTime;
			LastPlayTime = t.LastPlayTime;

			IsProcNEqExec = t.IsProcNEqExec;
			IsPathExist = t.IsPathExist;
			ExecPath = t.ExecPath;
			ProcPath = t.ProcPath;
			Status = t.Status;
            DisplayInfo = t.DisplayInfo;

			uid = t.uid;
		}

		public bool run()
		{
			if (!IsPathExist) return false;
            DisplaySettings.AdjustDisplay(DisplayInfo, this);
			ProcessStartInfo start = new ProcessStartInfo(ExecPath);
			start.CreateNoWindow = false;
			start.RedirectStandardOutput = true;
			start.RedirectStandardInput = true;
			start.UseShellExecute = false;
			start.WorkingDirectory = new FileInfo(ExecPath).DirectoryName;
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
