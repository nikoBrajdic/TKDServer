using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        [JsonProperty("contestName")]
        public string ContestName { get; set; }
        [JsonIgnore]
        public string Date { get => DateTime.Today.ToString("dd. MMMM yyyy.", CultureInfo.InvariantCulture); }
    }
}
