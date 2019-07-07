using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace TKD.App
{
    public class Device
    {
        public int Id { get; set; }
        public string Mac { get; set; }
        public WebSocket Handle { get; set; }
        public bool Enabled { get; set; }
    }
}
