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

		public void getEsInfo(GameInfo g)
		{
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = "SELECT * FROM erogamescape WHERE id='" + g.ErogameScapeID + "' ";
				SQLiteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					g.Title = reader["title"].ToString();
					g.Brand = reader["brand"].ToString();
					g.SaleDay = DateTime.ParseExact(reader["saleday"].ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
				}
				conn.Close();
			}
		}

		public List<GameInfo> getAllEsInfo()
		{
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = "SELECT * FROM erogamescape ";
				SQLiteDataReader reader = command.ExecuteReader();
				List<GameInfo> ans = new List<GameInfo>();
				while (reader.Read())
				{
					GameInfo g = new GameInfo();
					g.ErogameScapeID = Convert.ToInt32(reader["id"].ToString());
					g.Title = reader["title"].ToString();
					g.Brand = reader["brand"].ToString();
					string sday = reader["saleday"].ToString();
					if (sday != "") {
						g.SaleDay = DateTime.ParseExact(sday, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
					}
					ans.Add(g);
				}
				conn.Close();
				return ans;
			}
		}

		public void update(List<GameInfo> infoList)
		{
			using (SQLiteCommand insertRngCmd = (SQLiteCommand)conn.CreateCommand())
			{
				insertRngCmd.CommandText = @"INSERT or REPLACE INTO erogamescape VALUES(@ID,@TITLE,@SALEDAY,@BRAND)";
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					foreach (GameInfo info in infoList)
					{
						insertRngCmd.Parameters.AddWithValue("@ID", info.ErogameScapeID);
						insertRngCmd.Parameters.AddWithValue("@TITLE", info.Title);
						insertRngCmd.Parameters.AddWithValue("@SALEDAY", info.SaleDay.ToString("yyyy-MM-dd"));
						insertRngCmd.Parameters.AddWithValue("@BRAND", info.Brand);
						insertRngCmd.ExecuteNonQuery();
					}
					transaction.Commit();
				}
				conn.Close();
			}
		}
	}
}
