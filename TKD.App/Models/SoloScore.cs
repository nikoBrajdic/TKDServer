namespace TKD.App.Models
{
    public class SoloScore
    {
        public int ID { get; set; }

        public string Type { get; set; }

        public int Index { get; set; }

        public double? Value { get; set; }

        public int ScoreId { get; set; }

        public virtual Score Score { get; set; }
    }
}
