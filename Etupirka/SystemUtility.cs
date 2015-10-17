using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Etupirka
{
	public static class Utility
	{


		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

		[DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
		public static extern IntPtr GetForegroundWindow();

		public static string strSourcePath= System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\";

		public static EsInfo esInfo=new EsInfo(strSourcePath+"\\esdata.dat");

		public static Random randomGen=new Random(Guid.NewGuid().GetHashCode());

		public static string userDBPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\"+"user.db";
		public static string infoDBPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\"+"data.db";

		public static byte[] Decompress(byte[] data)
		{
			//Console.WriteLine("Start decompress");
			using (var compressedStream = new MemoryStream(data))
			using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
			using (var resultStream = new MemoryStream())
			{
				zipStream.CopyTo(resultStream);
				return resultStream.ToArray();
			}
		}



	}

	public class EsInfo
	{
		public List<int> id;
		public List<string> name;
		public List<string> saleday;
		public List<string> brand;
		public EsInfo(string path)
		{
			var reader = new StreamReader(File.OpenRead(path));
			id = new List<int>();
			name = new List<string>();
			saleday = new List<string>();
			brand = new List<string>();
			while (!reader.EndOfStream)
			{
				var line = reader.ReadLine();
				var values = line.Split('\t');
				id.Add(Int32.Parse(values[0]));
				name.Add(values[1]);
				saleday.Add(values[2]);
				brand.Add(values[3]);
			}
		}
		public int getTermID(int i){
			return id.BinarySearch(i);
		}

	}
}
