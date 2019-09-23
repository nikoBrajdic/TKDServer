using System.ComponentModel;
using System.Runtime.CompilerServices;
using TKD.App.Models;

namespace TKD.App
{
    public class Performer : INotifyPropertyChanged
    {
        private Contestant contestant;
        public Contestant Contestant {
            get => contestant;
            set {
                contestant = value;
                OnPropertyChanged();
            }
        }
        private Score score;
        public Score Score {
            get => score;
            set
            {
                score = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}