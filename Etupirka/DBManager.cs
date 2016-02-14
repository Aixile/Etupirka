using Etupirka.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Etupirka
{
	class DBManager
	{
		private string db_file;
		public SQLiteConnection conn;
		public DBManager(string file)
		{
			db_file = file;
			conn = new SQLiteConnection("Data Source=" + db_file);
			if (!File.Exists(db_file))
			{
				conn.Open();
				using (SQLiteCommand command = conn.CreateCommand())
				{
					command.CommandText = @"CREATE TABLE `games` (
										`uid`	TEXT NOT NULL,
										`title`	TEXT,
										`brand`	TEXT,
										`saleday`	TEXT,
										`esid`	INTEGER DEFAULT 0,
										PRIMARY KEY(uid));";
					command.ExecuteNonQuery();
					command.CommandText = @"CREATE TABLE `playtime` (
										`datetime`	TEXT NOT NULL,
										`game`	TEXT NOT NULL,
										`playtime`	INTEGER NOT NULL DEFAULT 0,
										PRIMARY KEY(datetime,game))";
					command.ExecuteNonQuery();
					command.CommandText = @"CREATE TABLE `gametimeinfo` (
										`uid`	TEXT,
										`playtime`	INTEGER DEFAULT 0,
										`firstplay`	TEXT,
										`lastplay`	TEXT,
										PRIMARY KEY(uid))";
					command.ExecuteNonQuery();
					command.CommandText = @"CREATE TABLE `gameexecinfo` (
										`uid`	TEXT,
										`proc_neq_exec`	INTEGER NOT NULL DEFAULT 0,
										`procpath`	TEXT,
										`execpath`	TEXT,
										PRIMARY KEY(uid))";
					command.ExecuteNonQuery();
                    command.CommandText = @"CREATE TABLE `gamedisplayinfo` (
										`uid`	TEXT,
										`device_id`	TEXT,
										`scaling`	INTEGER NOT NULL DEFAULT 0,
										`enabled`	INTEGER NOT NULL DEFAULT 0,
										PRIMARY KEY(uid, device_id))";
                    command.ExecuteNonQuery();
                }
				conn.Close();
			}
		}
		public void UpdateTimeNow(string game, int value)
		{
			conn.Open();
			using (SQLiteCommand command = conn.CreateCommand())
			{
				command.CommandText = @"INSERT or IGNORE INTO playtime 
										VALUES( date('"+DateTime.Today.ToString("yyyy-MM-dd")+@"') , '" + game + @"',0);
										UPDATE playtime SET playtime=playtime+" + value + @" 
										WHERE datetime=date('" + DateTime.Today.ToString("yyyy-MM-dd") + @"') and 
										game='" + game + @"'";
				command.ExecuteNonQuery();
			}
			conn.Close();
		}

		public void InsertOrReplaceTime(string date, string game, int value)
		{
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = @"INSERT or REPLACE INTO playtime 
										VALUES( date('" + date + @"') , '" + game + @"'," + value + @");";
				command.ExecuteNonQuery();
				conn.Close();
			}
		}

		public void InsertOrReplaceTime(Dictionary<string, TimeData> timeDict)
		{
			using (SQLiteCommand insertRngCmd = (SQLiteCommand)conn.CreateCommand())
			{
				insertRngCmd.CommandText = @"INSERT or REPLACE INTO playtime VALUES(@DATE,@GAME,@PLAYTIME)";
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					foreach (KeyValuePair<string, TimeData> i in timeDict)
					{
						foreach (KeyValuePair<string, int> j in i.Value.d)
						{
							DateTime dt = new DateTime();
							try
							{
								dt = DateTime.ParseExact(i.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture);
							}
							catch
							{
								try
								{
									dt = DateTime.ParseExact(i.Key, "yyyy-M-dd", CultureInfo.InvariantCulture);
								}
								catch
								{
									try
									{
										dt = DateTime.ParseExact(i.Key, "M-dd-yyyy", CultureInfo.InvariantCulture);
									}
									catch
									{
										try
										{
											dt = DateTime.ParseExact(i.Key, "MM-dd-yyyy", CultureInfo.InvariantCulture);

										}
										catch
										{

										}
									}
								}
							} 
							insertRngCmd.Parameters.AddWithValue("@DATE", dt.ToString("yyyy-MM-dd"));
							insertRngCmd.Parameters.AddWithValue("@GAME", j.Key);
							insertRngCmd.Parameters.AddWithValue("@PLAYTIME", j.Value);
							insertRngCmd.ExecuteNonQuery();
						}
					}

					transaction.Commit();
				}
				conn.Close();
			}
		}

		public void AddPlayTime(Dictionary<string, TimeData> timeDict)
		{
			using (SQLiteCommand insertRngCmd = (SQLiteCommand)conn.CreateCommand())
			{
				insertRngCmd.CommandText = @"INSERT or IGNORE INTO playtime VALUES(@DATE,@GAME,0);
										UPDATE playtime SET playtime=playtime+@PLAYTIME WHERE datetime=@DATE and game=@GAME";
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					foreach (KeyValuePair<string, TimeData> i in timeDict)
					{
						foreach (KeyValuePair<string, int> j in i.Value.d)
						{
							DateTime dt = new DateTime();
							try
							{
								dt = DateTime.ParseExact(i.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture);
							}
							catch
							{
								try
								{
									dt = DateTime.ParseExact(i.Key, "yyyy-M-dd", CultureInfo.InvariantCulture);
								}
								catch
								{
									try
									{
										dt = DateTime.ParseExact(i.Key, "M-dd-yyyy", CultureInfo.InvariantCulture);
									}
									catch
									{
										try
										{
											dt = DateTime.ParseExact(i.Key, "MM-dd-yyyy", CultureInfo.InvariantCulture);
										
										}
										catch
										{

										}
									}
								}
							}
							insertRngCmd.Parameters.AddWithValue("@DATE", dt.ToString("yyyy-MM-dd"));
							insertRngCmd.Parameters.AddWithValue("@GAME", j.Key);
							insertRngCmd.Parameters.AddWithValue("@PLAYTIME", j.Value);
							insertRngCmd.ExecuteNonQuery();
						}
					}

					transaction.Commit();
				}
				conn.Close();
			}
		}
		public Dictionary<string, TimeData> GetPlayTime()
		{
			Dictionary<string, TimeData> timeDict = new Dictionary<string, TimeData>(); 
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = "SELECT * FROM playtime";
				SQLiteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					string date = reader["datetime"].ToString();
					string game = reader["game"].ToString();
					int playtime = Convert.ToInt32(reader["playtime"].ToString());
					if (!timeDict.ContainsKey(date))
					{
						timeDict.Add(date, new TimeData());

					}
					timeDict[date].AddTime(game, playtime);
				}

				conn.Close();
			}
			return timeDict;
		}

		public List<TimeSummary> QueryGamePlayTime(string uid)
		{
			List<TimeSummary> tlist = new List<TimeSummary>();
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = "SELECT * FROM playtime WHERE game='"+uid+"' ORDER BY datetime DESC";
				SQLiteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					string date = reader["datetime"].ToString();
					int playtime = Convert.ToInt32(reader["playtime"].ToString());
					tlist.Add(new TimeSummary(date, playtime));
				}
				conn.Close();
			}
			return tlist;
		}

		public Dictionary<string, TimeData> GetPlayTime(List<GameExecutionInfo> a)
		{
			Dictionary<string, TimeData> timeDict = new Dictionary<string, TimeData>();
			string str = " WHERE game=''";
			foreach(GameExecutionInfo i in a){
				str=str+" OR game='"+i.UID+"'";
			}
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = "SELECT * FROM playtime "+str;
				SQLiteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					string date = reader["datetime"].ToString();
					string game = reader["game"].ToString();
					int playtime = Convert.ToInt32(reader["playtime"].ToString());
					if (!timeDict.ContainsKey(date))
					{
						timeDict.Add(date, new TimeData());

					}
					timeDict[date].AddTime(game, playtime);
				}

				conn.Close();
			}
			return timeDict;
		}

		public void InsertOrReplaceGame(GameExecutionInfo i)
		{
			using (SQLiteCommand insertGameRngCmd = (SQLiteCommand)conn.CreateCommand())
			{
				using (SQLiteCommand insertTimeRngCmd = (SQLiteCommand)conn.CreateCommand())
				{
					using (SQLiteCommand insertExecRngCmd = (SQLiteCommand)conn.CreateCommand())
					{
						conn.Open();
						insertGameRngCmd.CommandText = @"INSERT or REPLACE INTO games VALUES(@UID,@TITLE,@BRAND,@SALEDAY,@ESID)";
						insertTimeRngCmd.CommandText = @"INSERT or REPLACE INTO gametimeinfo VALUES(@UID,@PLAYTIME,@FIRSTPLAY,@LASTPLAY)";
						insertExecRngCmd.CommandText = @"INSERT or REPLACE INTO gameexecinfo VALUES(@UID,@P_N_E,@PROCPATH,@EXECPATH)";

						insertGameRngCmd.Parameters.AddWithValue("@UID", i.UID);
						insertGameRngCmd.Parameters.AddWithValue("@TITLE", i.Title);
						insertGameRngCmd.Parameters.AddWithValue("@BRAND", i.Brand);
						insertGameRngCmd.Parameters.AddWithValue("@SALEDAY", i.SaleDay.ToString("yyyy-MM-dd"));
						insertGameRngCmd.Parameters.AddWithValue("@ESID", i.ErogameScapeID);
						insertGameRngCmd.ExecuteNonQuery();

						insertTimeRngCmd.Parameters.AddWithValue("@UID", i.UID);
						insertTimeRngCmd.Parameters.AddWithValue("@PLAYTIME", i.TotalPlayTime);
						insertTimeRngCmd.Parameters.AddWithValue("@FIRSTPLAY", i.FirstPlayTime.ToString("yyyy-MM-dd HH:mm:ss"));
						insertTimeRngCmd.Parameters.AddWithValue("@LASTPLAY", i.LastPlayTime.ToString("yyyy-MM-dd HH:mm:ss"));
						insertTimeRngCmd.ExecuteNonQuery();

						insertExecRngCmd.Parameters.AddWithValue("@UID", i.UID);
						insertExecRngCmd.Parameters.AddWithValue("@P_N_E", (i.IsProcNEqExec ? 1 : 0));
						insertExecRngCmd.Parameters.AddWithValue("@PROCPATH", i.ProcPath);
						insertExecRngCmd.Parameters.AddWithValue("@EXECPATH", i.ExecPath);
						insertExecRngCmd.ExecuteNonQuery();
						conn.Close();
					}
				}
			}
		}

		public void UpdateGame(GameExecutionInfo i)
		{
			using (SQLiteCommand insertGameRngCmd = (SQLiteCommand)conn.CreateCommand())
			{
				using (SQLiteCommand insertTimeRngCmd = (SQLiteCommand)conn.CreateCommand())
				{
					using (SQLiteCommand insertExecRngCmd = (SQLiteCommand)conn.CreateCommand())
					{
						conn.Open();
						insertGameRngCmd.CommandText = @"UPDATE games SET title=@TITLE,brand=@BRAND,saleday=@SALEDAY,esid=@ESID WHERE uid=@UID";
						insertTimeRngCmd.CommandText = @"UPDATE gametimeinfo SET playtime=@PLAYTIME,firstplay=@FIRSTPLAY,lastplay=@LASTPLAY WHERE uid=@UID";
						insertExecRngCmd.CommandText = @"UPDATE gameexecinfo SET proc_neq_exec=@P_N_E,procpath=@PROCPATH,execpath=@EXECPATH WHERE uid=@UID";

						insertGameRngCmd.Parameters.AddWithValue("@UID", i.UID);
						insertGameRngCmd.Parameters.AddWithValue("@TITLE", i.Title);
						insertGameRngCmd.Parameters.AddWithValue("@BRAND", i.Brand);
						insertGameRngCmd.Parameters.AddWithValue("@SALEDAY", i.SaleDay.ToString("yyyy-MM-dd"));
						insertGameRngCmd.Parameters.AddWithValue("@ESID", i.ErogameScapeID);
						insertGameRngCmd.ExecuteNonQuery();

						insertTimeRngCmd.Parameters.AddWithValue("@UID", i.UID);
						insertTimeRngCmd.Parameters.AddWithValue("@PLAYTIME", i.TotalPlayTime);
						insertTimeRngCmd.Parameters.AddWithValue("@FIRSTPLAY", i.FirstPlayTime.ToString("yyyy-MM-dd HH:mm:ss"));
						insertTimeRngCmd.Parameters.AddWithValue("@LASTPLAY", i.LastPlayTime.ToString("yyyy-MM-dd HH:mm:ss"));
						insertTimeRngCmd.ExecuteNonQuery();

						insertExecRngCmd.Parameters.AddWithValue("@UID", i.UID);
						insertExecRngCmd.Parameters.AddWithValue("@P_N_E", (i.IsProcNEqExec ? 1 : 0));
						insertExecRngCmd.Parameters.AddWithValue("@PROCPATH", i.ProcPath);
						insertExecRngCmd.Parameters.AddWithValue("@EXECPATH", i.ExecPath);
						insertExecRngCmd.ExecuteNonQuery();
						conn.Close();
					}
				}
			}
		}


		public void InsertOrReplaceGame(ObservableCollection<GameExecutionInfo> items)
		{
			using (SQLiteCommand insertGameRngCmd = (SQLiteCommand)conn.CreateCommand())
			{
				using (SQLiteCommand insertTimeRngCmd = (SQLiteCommand)conn.CreateCommand())
				{
					using (SQLiteCommand insertExecRngCmd = (SQLiteCommand)conn.CreateCommand())
					{
						insertGameRngCmd.CommandText = @"INSERT or REPLACE INTO games VALUES(@UID,@TITLE,@BRAND,@SALEDAY,@ESID)";
						insertTimeRngCmd.CommandText = @"INSERT or REPLACE INTO gametimeinfo VALUES(@UID,@PLAYTIME,@FIRSTPLAY,@LASTPLAY)";
						insertExecRngCmd.CommandText = @"INSERT or REPLACE INTO gameexecinfo VALUES(@UID,@P_N_E,@PROCPATH,@EXECPATH)";

						conn.Open();
						using (var transaction = conn.BeginTransaction())
						{
							foreach (var i in items)
							{
								insertGameRngCmd.Parameters.AddWithValue("@UID", i.UID);
								insertGameRngCmd.Parameters.AddWithValue("@TITLE", i.Title);
								insertGameRngCmd.Parameters.AddWithValue("@BRAND", i.Brand);
								insertGameRngCmd.Parameters.AddWithValue("@SALEDAY", i.SaleDay.ToString("yyyy-MM-dd"));
								insertGameRngCmd.Parameters.AddWithValue("@ESID", i.ErogameScapeID);
								insertGameRngCmd.ExecuteNonQuery();

								insertTimeRngCmd.Parameters.AddWithValue("@UID", i.UID);
								insertTimeRngCmd.Parameters.AddWithValue("@PLAYTIME", i.TotalPlayTime);
								insertTimeRngCmd.Parameters.AddWithValue("@FIRSTPLAY", i.FirstPlayTime.ToString("yyyy-MM-dd HH:mm:ss"));
								insertTimeRngCmd.Parameters.AddWithValue("@LASTPLAY", i.LastPlayTime.ToString("yyyy-MM-dd HH:mm:ss"));
								insertTimeRngCmd.ExecuteNonQuery();

								insertExecRngCmd.Parameters.AddWithValue("@UID", i.UID);
								insertExecRngCmd.Parameters.AddWithValue("@P_N_E", (i.IsProcNEqExec ? 1 : 0));
								insertExecRngCmd.Parameters.AddWithValue("@PROCPATH", i.ProcPath);
								insertExecRngCmd.Parameters.AddWithValue("@EXECPATH", i.ExecPath);
								insertExecRngCmd.ExecuteNonQuery();
							}
							transaction.Commit();
						}
						conn.Close();
					}
				}
			}
		}

		public void InsertOrIgnoreGame(ObservableCollection<GameExecutionInfo> items)
		{
			using (SQLiteCommand insertGameRngCmd = (SQLiteCommand)conn.CreateCommand())
			{
				using (SQLiteCommand insertTimeRngCmd = (SQLiteCommand)conn.CreateCommand())
				{
					using (SQLiteCommand insertExecRngCmd = (SQLiteCommand)conn.CreateCommand())
					{
						insertGameRngCmd.CommandText = @"INSERT or IGNORE INTO games VALUES(@UID,@TITLE,@BRAND,@SALEDAY,@ESID)";
						insertTimeRngCmd.CommandText = @"INSERT or IGNORE INTO gametimeinfo VALUES(@UID,0,@FIRSTPLAY,@LASTPLAY);
														 UPDATE gametimeinfo SET playtime=playtime+@PLAYTIME WHERE uid=@UID";
						insertExecRngCmd.CommandText = @"INSERT or IGNORE INTO gameexecinfo VALUES(@UID,@P_N_E,@PROCPATH,@EXECPATH)";

						conn.Open();
						using (var transaction = conn.BeginTransaction())
						{
							foreach (var i in items)
							{
								insertGameRngCmd.Parameters.AddWithValue("@UID", i.UID);
								insertGameRngCmd.Parameters.AddWithValue("@TITLE", i.Title);
								insertGameRngCmd.Parameters.AddWithValue("@BRAND", i.Brand);
								insertGameRngCmd.Parameters.AddWithValue("@SALEDAY", i.SaleDay.ToString("yyyy-MM-dd"));
								insertGameRngCmd.Parameters.AddWithValue("@ESID", i.ErogameScapeID);
								insertGameRngCmd.ExecuteNonQuery();

								insertTimeRngCmd.Parameters.AddWithValue("@UID", i.UID);
								insertTimeRngCmd.Parameters.AddWithValue("@PLAYTIME", i.TotalPlayTime);
								insertTimeRngCmd.Parameters.AddWithValue("@FIRSTPLAY", i.FirstPlayTime.ToString("yyyy-MM-dd HH:mm:ss"));
								insertTimeRngCmd.Parameters.AddWithValue("@LASTPLAY", i.LastPlayTime.ToString("yyyy-MM-dd HH:mm:ss"));
								insertTimeRngCmd.ExecuteNonQuery();

								insertExecRngCmd.Parameters.AddWithValue("@UID", i.UID);
								insertExecRngCmd.Parameters.AddWithValue("@P_N_E", (i.IsProcNEqExec ? 1 : 0));
								insertExecRngCmd.Parameters.AddWithValue("@PROCPATH", i.ProcPath);
								insertExecRngCmd.Parameters.AddWithValue("@EXECPATH", i.ExecPath);
								insertExecRngCmd.ExecuteNonQuery();
							}
							transaction.Commit();
						}
						conn.Close();
					}
				}
			}
		}

		public void UpdateGameInfoAndExec(GameExecutionInfo i)
		{
			using (SQLiteCommand insertGameRngCmd = (SQLiteCommand)conn.CreateCommand())
			{
				using (SQLiteCommand insertExecRngCmd = (SQLiteCommand)conn.CreateCommand())
				{
					conn.Open();
					insertGameRngCmd.CommandText = @"UPDATE games SET title=@TITLE,brand=@BRAND,saleday=@SALEDAY,esid=@ESID WHERE uid=@UID";
					insertExecRngCmd.CommandText = @"UPDATE gameexecinfo SET proc_neq_exec=@P_N_E,procpath=@PROCPATH,execpath=@EXECPATH WHERE uid=@UID";

					insertGameRngCmd.Parameters.AddWithValue("@UID", i.UID);
					insertGameRngCmd.Parameters.AddWithValue("@TITLE", i.Title);
					insertGameRngCmd.Parameters.AddWithValue("@BRAND", i.Brand);
					insertGameRngCmd.Parameters.AddWithValue("@SALEDAY", i.SaleDay.ToString("yyyy-MM-dd"));
					insertGameRngCmd.Parameters.AddWithValue("@ESID", i.ErogameScapeID);
					insertGameRngCmd.ExecuteNonQuery();

					insertExecRngCmd.Parameters.AddWithValue("@UID", i.UID);
					insertExecRngCmd.Parameters.AddWithValue("@P_N_E", (i.IsProcNEqExec ? 1 : 0));
					insertExecRngCmd.Parameters.AddWithValue("@PROCPATH", i.ProcPath);
					insertExecRngCmd.Parameters.AddWithValue("@EXECPATH", i.ExecPath);
					insertExecRngCmd.ExecuteNonQuery();

                    foreach (var device in i.DisplayInfo.devices)
                    {
                        using (SQLiteCommand insertDisplayCmd = new SQLiteCommand(@"INSERT OR REPLACE INTO gamedisplayinfo VALUES (@uid, @device_id, @scaling, @enabled)", conn))
                        {
                            insertDisplayCmd.Parameters.AddWithValue("@uid", i.UID);
                            insertDisplayCmd.Parameters.AddWithValue("@device_id", device.DeviceID);
                            insertDisplayCmd.Parameters.AddWithValue("@scaling", device.Scaling);
                            insertDisplayCmd.Parameters.AddWithValue("@enabled", device.Enabled ? 1 : 0);
                            insertDisplayCmd.ExecuteNonQuery();
                        }
                    }

					conn.Close();
				}
			}
		}

		public void LoadGame(ObservableCollection<GameExecutionInfo> items)
		{
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = "SELECT * FROM games g, gametimeinfo t, gameexecinfo e WHERE g.uid=e.uid AND g.uid=t.uid";
				SQLiteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					GameExecutionInfo i = new GameExecutionInfo(
						 reader["uid"].ToString(),
						 reader["title"].ToString(),
						 reader["brand"].ToString(),
						 Convert.ToInt32(reader["esid"].ToString()),
						 DateTime.ParseExact(reader["saleday"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
						 Convert.ToInt32(reader["playtime"].ToString()),
						 DateTime.ParseExact(reader["firstplay"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
						 DateTime.ParseExact(reader["lastplay"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
						 (Convert.ToInt32(reader["proc_neq_exec"].ToString()) == 1 ? true : false),
						 reader["procpath"].ToString(),
						 reader["execpath"].ToString(),
                         LoadGameDisplayInfo(reader["uid"].ToString())
                         );
					items.Add(i);

				}

				conn.Close();
			}
		}

        public DisplayInfo LoadGameDisplayInfo(string UID)
        {
            DisplayInfo displayInfo = new DisplayInfo();

            using (SQLiteCommand command = conn.CreateCommand())
            {
                command.CommandText = "SELECT * FROM gamedisplayinfo WHERE uid = @uid";
                command.Parameters.AddWithValue("@uid", UID);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    displayInfo.devices.Add(new DisplayDeviceInfo() {
                        DeviceID = reader["device_id"].ToString(),
                        Scaling = int.Parse(reader["scaling"].ToString()),
                        Enabled = reader["enabled"].ToString() == "1" ? true : false
                    });
                }
            }

            return displayInfo;
        }

		public void DeleteGame(string uid)
		{
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = @"DELETE FROM games WHERE uid= '" + uid + @"';" +
									@"DELETE FROM gametimeinfo WHERE uid= '" + uid + @"';" +
									@"DELETE FROM gameexecinfo WHERE uid= '" + uid + @"';" +
                                    @"DELETE FROM gamedisplayinfo WHERE uid= '" + uid + @"';";
                command.ExecuteNonQuery();
				conn.Close();
			}
		}

		public void UpdateGameTimeInfo(string uid, int time, DateTime firstplay, DateTime lastplay)
		{
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = @"UPDATE gametimeinfo SET playtime=@PLAYTIME,firstplay=@FIRSTPLAY,lastplay=@LASTPLAY WHERE uid=@UID";
				command.Parameters.AddWithValue("@UID", uid);
				command.Parameters.AddWithValue("@PLAYTIME", time);
				command.Parameters.AddWithValue("@FIRSTPLAY", firstplay.ToString("yyyy-MM-dd HH:mm:ss"));
				command.Parameters.AddWithValue("@LASTPLAY", lastplay.ToString("yyyy-MM-dd HH:mm:ss"));
				command.ExecuteNonQuery();
				conn.Close();
			}
		}

		/*
		public void InsertOrReplaceGameInfo(string uid, string title, string brand, DateTime saleday,int ESID)
		{
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = @"INSERT or REPLACE INTO games 
										 VALUES('" + uid + @"','" + title + @"','" + brand + @"','" + saleday.ToShortDateString().Replace('/', '-') + @"',"+ESID+"');";
				command.ExecuteNonQuery();
				conn.Close();
			}
		}



		public void InsertOrReplaceGameExecInfo(string uid, bool proc_neq_exec, string procpath, string execpath)
		{
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = @"INSERT or REPLACE INTO gameexecinfo
										 VALUES('" + uid + @"','" + (proc_neq_exec?1:0) + @"','" + procpath + @"','" + execpath + "');";
				command.ExecuteNonQuery();
				conn.Close();
			}
		}

		public void InsertOrReplaceGameInfo(ObservableCollection<GameExecutionInfo> items)
		{
			SQLiteCommand insertRngCmd = (SQLiteCommand)conn.CreateCommand();
			insertRngCmd.CommandText = @"INSERT or REPLACE INTO games VALUES(@UID,@TITLE,@BRAND,@SALEDAY,@ESID)";
			conn.Open();
			var transaction = conn.BeginTransaction();
			foreach (var i in items)
			{
				insertRngCmd.Parameters.AddWithValue("@UID", i.UID);
				insertRngCmd.Parameters.AddWithValue("@TITLE", i.Title);
				insertRngCmd.Parameters.AddWithValue("@BRAND", i.Brand);
				insertRngCmd.Parameters.AddWithValue("@SALEDAY", i.SaleDay.ToShortDateString().Replace('/', '-'));
				insertRngCmd.Parameters.AddWithValue("@ESID", i.ErogameScapeID);
				insertRngCmd.ExecuteNonQuery();
			}
			transaction.Commit();
			conn.Close();
		}*/

	}

}