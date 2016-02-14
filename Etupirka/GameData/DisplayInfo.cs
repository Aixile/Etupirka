using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Etupirka
{
    public class DisplayInfo
    {
        public ICollection<DisplayDeviceInfo> devices = new List<DisplayDeviceInfo>();
    }

    public class DisplayDeviceInfo
    {
        public string DeviceString { get; set; }
        public string DeviceID { get; set; }
        public int Scaling { get; set; }
        public bool Enabled { get; set; }
    }
}
