namespace TKD.App.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Score
    {
        public Score() {}
        public Score(int contestantId, int round)
        {
            ContestantId = contestantId;
            Round = round;
        }
        public int ID { get; set; }

        public int Round { get; set; } = 1;

        public int Index { get; set; } = 1;

        public double? MinorMean { get; set; }

        public double? MinorTotal { get; set; }

        public double? GrandTotal { get; set; }

        public double? AccuracyMinorTotal { get; set; }

        public double? PresentationMinorTotal { get; set; }

        public double? AccuracyGrandTotal { get; set; }

        public double? PresentationGrandTotal { get; set; }

        public double? Presentation1 { get; set; }

        public double? Presentation2 { get; set; }

        public double? Presentation3 { get; set; }

        public double? Presentation4 { get; set; }

        public double? Presentation5 { get; set; }

        public double? Presentation6 { get; set; }

        public double? Presentation7 { get; set; }

        public double? Presentation8 { get; set; }

        public double? Presentation9 { get; set; }

        public double? Accuracy1 { get; set; }

        public double? Accuracy2 { get; set; }

        public double? Accuracy3 { get; set; }

        public double? Accuracy4 { get; set; }

        public double? Accuracy5 { get; set; }

        public double? Accuracy6 { get; set; }

        public double? Accuracy7 { get; set; }

        public double? Accuracy8 { get; set; }

        public double? Accuracy9 { get; set; }

        public int ContestantId { get; set; }

        public virtual Contestant Contestant { get; set; }
    }
}
