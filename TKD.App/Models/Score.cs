namespace TKD.App.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public partial class Score : INotifyPropertyChanged
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Score()
        {
            SoloScores = new HashSet<SoloScore>();
        }
        public Score(Contestant contestant, int round)
        {
            ContestantId = contestant.ID;
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

        public int ContestantId { get; set; }

        public virtual Contestant Contestant { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SoloScore> SoloScores { get; set; }

        private SoloScore GetScore(string type, int index) =>
            MainWindow.Context.SoloScores.Local.FirstOrDefault(ss => ss.Type == type && ss.Index == index && ss.Score == this);

        private void SetScore(string type, int index, double? value)
        {
            var score = GetScore(type, index);
            if (score != null)
            {
                score.Value = value;
            }
            else
            {
                MainWindow.Context.SoloScores.Local.Add(new SoloScore {
                    Index = index,
                    Score = this,
                    Type = type,
                    Value = value
                });
            }
            OnPropertyChanged(type + index);
        }

        public double? Accuracy1 { get => GetScore("Accuracy", 1)?.Value; set => SetScore("Accuracy", 1, value); }
        public double? Accuracy2 { get => GetScore("Accuracy", 2)?.Value; set => SetScore("Accuracy", 2, value); }
        public double? Accuracy3 { get => GetScore("Accuracy", 3)?.Value; set => SetScore("Accuracy", 3, value); }
        public double? Accuracy4 { get => GetScore("Accuracy", 4)?.Value; set => SetScore("Accuracy", 4, value); }
        public double? Accuracy5 { get => GetScore("Accuracy", 5)?.Value; set => SetScore("Accuracy", 5, value); }
        public double? Accuracy6 { get => GetScore("Accuracy", 6)?.Value; set => SetScore("Accuracy", 6, value); }
        public double? Accuracy7 { get => GetScore("Accuracy", 7)?.Value; set => SetScore("Accuracy", 7, value); }
        public double? Accuracy8 { get => GetScore("Accuracy", 8)?.Value; set => SetScore("Accuracy", 8, value); }
        public double? Accuracy9 { get => GetScore("Accuracy", 9)?.Value; set => SetScore("Accuracy", 9, value); }

        public double? Presentation1 { get => GetScore("Presentation", 1)?.Value; set => SetScore("Presentation", 1, value); }
        public double? Presentation2 { get => GetScore("Presentation", 2)?.Value; set => SetScore("Presentation", 2, value); }
        public double? Presentation3 { get => GetScore("Presentation", 3)?.Value; set => SetScore("Presentation", 3, value); }
        public double? Presentation4 { get => GetScore("Presentation", 4)?.Value; set => SetScore("Presentation", 4, value); }
        public double? Presentation5 { get => GetScore("Presentation", 5)?.Value; set => SetScore("Presentation", 5, value); }
        public double? Presentation6 { get => GetScore("Presentation", 6)?.Value; set => SetScore("Presentation", 6, value); }
        public double? Presentation7 { get => GetScore("Presentation", 7)?.Value; set => SetScore("Presentation", 7, value); }
        public double? Presentation8 { get => GetScore("Presentation", 8)?.Value; set => SetScore("Presentation", 8, value); }
        public double? Presentation9 { get => GetScore("Presentation", 9)?.Value; set => SetScore("Presentation", 9, value); }
        public double? DisMinorMean {
            get => MinorMean;
            set
            {
                MinorMean = value;
                OnPropertyChanged("MinorMean");
            }
        }
        public double? DisMinorTotal {
            get => MinorTotal;
            set
            {
                MinorTotal = value;
                OnPropertyChanged("MinorTotal");
            }
        }
        public double? DisGrandTotal {
            get => MinorMean;
            set
            {
                MinorMean = value;
                OnPropertyChanged("MinorMean");
            }
        }
        public double? DisAccuracyMinorTotal {
            get => AccuracyMinorTotal;
            set
            {
                AccuracyMinorTotal = value;
                OnPropertyChanged("AccuracyMinorTotal");
            }
        }
        public double? DisPresentationMinorTotal {
            get => PresentationMinorTotal;
            set
            {
                PresentationMinorTotal = value;
                OnPropertyChanged("PresentationMinorTotal");
            }
        }
        public double? DisAccuracyGrandTotal {
            get => AccuracyGrandTotal;
            set
            {
                AccuracyGrandTotal = value;
                OnPropertyChanged("AccuracyGrandTotal");
            }
        }
        public double? DisPresentationGrandTotal {
            get => PresentationGrandTotal;
            set
            {
                PresentationGrandTotal = value;
                OnPropertyChanged("PresentationGrandTotal ");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
