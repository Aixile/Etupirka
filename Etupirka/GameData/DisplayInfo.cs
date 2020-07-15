using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Etupirka
{
	
	public class DisplayInfo
    {
		[XmlArray("DeviceArray")]
		[XmlArrayItem("DeviceObject")]
		public List<DisplayDeviceInfo> devices = new List<DisplayDeviceInfo>();
    }

	[XmlType("Person")]
	public class DisplayDeviceInfo
	{
		[XmlElement("DeviceString")]
		public string DeviceString { get; set; }
		[XmlElement("DeviceID")]
		public string DeviceID { get; set; }
		[XmlElement("Scaling")]
		public int Scaling { get; set; }
		[XmlElement("Enabled")]
		public bool Enabled { get; set; }
	}
}
