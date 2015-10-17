using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Etupirka
{
	public class InformationManager
	{
		private string db_file;
		public SQLiteConnection conn;
		public InformationManager(string file)
		{
			create(file);

		}
		public void create(string file)
		{
			db_file = file;
			conn = new SQLiteConnection("Data Source=" + db_file);
			if (!File.Exists(db_file))
			{
				conn.Open();
				using (SQLiteCommand command = conn.CreateCommand())
				{
					command.CommandText = @"CREATE TABLE `erogamescape` (
										`id`	INTEGER NOT NULL,
										`title`	TEXT,
										`saleday`	TEXT,
										`brand`	TEXT,
										PRIMARY KEY(id));
										CREATE TABLE `tableinfo` (
										`tablename`	TEXT NOT NULL,
										`version`	INTEGER,
										PRIMARY KEY(tablename))";
					command.ExecuteNonQuery();
					command.CommandText = @"INSERT INTO  tableinfo VALUES('erogamescape',0)";
					command.ExecuteNonQuery();

				}
				conn.Close();
			}
		}
		void update()
		{
			try
			{

				string url = Properties.Settings.Default.databaseSyncServer;
				url=url.TrimEnd('\\')+"\\"+"esdata.gz";
				var decompressed = Utility.Decompress(NetworkUtility.GetData(url));
				string s = Encoding.UTF8.GetString(decompressed);
				string[] line = s.Split('\n');
				using (SQLiteCommand insertRngCmd = (SQLiteCommand)conn.CreateCommand())
				{
					insertRngCmd.CommandText = @"INSERT or REPLACE INTO erogamescape VALUES(@ID,@TITLE,@SALEDAY,@BRAND)";
					conn.Open();
					using (var transaction = conn.BeginTransaction())
					{
						foreach (string i in line)
						{
							string[] values = i.Split('\t');
							if (i.Count() < 4) continue;

							insertRngCmd.Parameters.AddWithValue("@ID", values[0]);
							insertRngCmd.Parameters.AddWithValue("@TITLE", values[1]);
							insertRngCmd.Parameters.AddWithValue("@SALEDAY", values[2]);
							insertRngCmd.Parameters.AddWithValue("@BRAND", values[3]);
							insertRngCmd.ExecuteNonQuery();
						}
						transaction.Commit();
					}
					conn.Close();
				}
			}
			catch
			{

			}
		}
	}
}
