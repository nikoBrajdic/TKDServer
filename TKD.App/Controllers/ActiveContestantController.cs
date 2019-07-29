using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using TKD.App.Models;
using TKD.App.Views;
using WebSocketSharp.Server;

namespace TKD.App.Controllers
{
    class ActiveContestantController : INotifyPropertyChanged
    {
        public MainWindow ParentWindow { get; set; }
        public TkdModel Context { get => MainWindow.Context; }
        public Performer Performer { get; set; }
        public Contestant Contestant { get => Performer.Contestant; }
        public Category Category { get => Contestant.Category; }
        public Score Score { get => Performer.Score; }
        public WebSocketServer WSSV { get => MainWindow.WSSV; }
        public ObservableCollection<Device> Devices { get => ParentWindow.ConnectedDevices; }
        private int RefNo { get => int.Parse(ParentWindow.RefereeCount.Text); }
        private DisplayScreen DisplayScreen { get => MainWindow.DisplayScreen; }
        private ContestantPage ContestantPage { get => MainWindow.ContestantPage; }

        DispatcherTimer timer;
        bool started = false;

        bool isEnabledL1;
        bool isEnabledL2;
        bool isEnabledL3;
        bool isEnabledL4;
        bool isEnabledL5;
        bool isEnabledL6;
        bool isEnabledL7;
        bool isEnabledL8;
        bool isEnabledL9;
        bool isEnabledR1;
        bool isEnabledR2;
        bool isEnabledR3;
        bool isEnabledR4;
        bool isEnabledR5;
        bool isEnabledR6;
        bool isEnabledR7;
        bool isEnabledR8;
        bool isEnabledR9;
        #region notifyPropertyChanged boilerplate
        bool IsEnabledL1 { get => RefNo < 1; set => SetEnabled(ref isEnabledL1, value); }
        bool IsEnabledL2 { get => RefNo < 2; set => SetEnabled(ref isEnabledL2, value); }
        bool IsEnabledL3 { get => RefNo < 3; set => SetEnabled(ref isEnabledL3, value); }
        bool IsEnabledL4 { get => RefNo < 4; set => SetEnabled(ref isEnabledL4, value); }
        bool IsEnabledL5 { get => RefNo < 5; set => SetEnabled(ref isEnabledL5, value); }
        bool IsEnabledL6 { get => RefNo < 6; set => SetEnabled(ref isEnabledL6, value); }
        bool IsEnabledL7 { get => RefNo < 7; set => SetEnabled(ref isEnabledL7, value); }
        bool IsEnabledL8 { get => RefNo < 8; set => SetEnabled(ref isEnabledL8, value); }
        bool IsEnabledL9 { get => RefNo < 9; set => SetEnabled(ref isEnabledL9, value); }
        bool IsEnabledR1 { get => RefNo < 1; set => SetEnabled(ref isEnabledR1, value); }
        bool IsEnabledR2 { get => RefNo < 2; set => SetEnabled(ref isEnabledR2, value); } 
        bool IsEnabledR3 { get => RefNo < 3; set => SetEnabled(ref isEnabledR3, value); }
        bool IsEnabledR4 { get => RefNo < 4; set => SetEnabled(ref isEnabledR4, value); }
        bool IsEnabledR5 { get => RefNo < 5; set => SetEnabled(ref isEnabledR5, value); }
        bool IsEnabledR6 { get => RefNo < 6; set => SetEnabled(ref isEnabledR6, value); }
        bool IsEnabledR7 { get => RefNo < 7; set => SetEnabled(ref isEnabledR7, value); }
        bool IsEnabledR8 { get => RefNo < 8; set => SetEnabled(ref isEnabledR8, value); }
        bool IsEnabledR9 { get => RefNo < 9; set => SetEnabled(ref isEnabledR9, value); }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void SetEnabled(ref bool enabled, bool value)
        {
            if (enabled != value)
            {
                enabled = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
