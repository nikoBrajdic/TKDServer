using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKD.App.Models
{
    public class Settings
    {
        [JsonProperty("refereeCount")]
        public int RefNo { get; set; }
        [JsonProperty("ip")]
        public string IP { get; set; }
    }
}
