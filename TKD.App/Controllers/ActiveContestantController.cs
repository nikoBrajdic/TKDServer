using System.ComponentModel;
using System.Runtime.CompilerServices;
using TKD.App.Models;
using TKD.App.Views;

namespace TKD.App.Controllers
{
    public class ActiveContestantController : INotifyPropertyChanged
    {
        private Performer performer;
        public Performer Performer {
            get => performer;
            set { performer = value; OnPropertyChanged(); }
        }
        public Score Score {
            get => Performer?.Score ?? new Score();
            set { Performer.Score = value; OnPropertyChanged(); }
        }

        private int RefNo { get => MainWindow.Settings.RefNo; }
        public bool IsEnabled1 { get => 1 <= RefNo; }
        public bool IsEnabled2 { get => 2 <= RefNo; }
        public bool IsEnabled3 { get => 3 <= RefNo; }
        public bool IsEnabled4 { get => 4 <= RefNo; }
        public bool IsEnabled5 { get => 5 <= RefNo; }
        public bool IsEnabled6 { get => 6 <= RefNo; }
        public bool IsEnabled7 { get => 7 <= RefNo; }
        public bool IsEnabled8 { get => 8 <= RefNo; }
        public bool IsEnabled9 { get => 9 <= RefNo; }


        #region notifyPropertyChanged boilerplate
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void RaisePropertyChanged(string name) => OnPropertyChanged(name);
        #endregion
    }
}
