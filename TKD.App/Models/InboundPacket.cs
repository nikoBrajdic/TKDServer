using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKD.App
{
    [Serializable]
    class InboundPacket
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public string Mac { get; set; }
        public int Battery { get; set; }
        public Scores Scores { get; set; } = null;

        public string Astr() => (Scores.Accuracy / 10f).ToString();
        public string Pstr() => (Scores.Presentation / 10f).ToString();
    }

    [Serializable]
    public class Scores
    {
        public Scores() { }
        public Scores(int acc, int pres)
        {
            Accuracy = acc;
            Presentation = pres;
        }
        public int Accuracy { get; set; }
        public int Presentation { get; set; }
    }
}
