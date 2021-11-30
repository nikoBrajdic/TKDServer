#define MVVM

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TKD.App.Controllers;
using TKD.App.Models;
using TKD.App.Views;
using WebSocketSharp.Server;
using System.Data.Entity;
using ExtensionsNamespace;


namespace TKD.App
{
    /// <summary>
    /// Interaction logic for ActiveContestant.xaml
    /// </summary>
    public partial class ActiveContestant : Window
    {
        public MainWindow ParentWindow { get; set; }
        public TkdModel Context { get => MainWindow.Context; }
        public Performer Performer {
            get => (DataContext as ActiveContestantController).Performer;
            set => (DataContext as ActiveContestantController).Performer = value;
        }
        public Contestant Contestant { get => Performer.Contestant; }
        public Category Category { get => Contestant.Category; }
        public Score Score { get => Performer.Score; }
        public WebSocketServer WSSV { get => MainWindow.WSSV; }
        public ObservableCollection<Device> Devices { get => ParentWindow.ConnectedDevices; }
        private int RefNo { get => int.Parse(ParentWindow.RefereeCount.Text); }
        private Audience DisplayScreen { get => MainWindow.Audience; }
        private ContestantPage ContestantPage { get => MainWindow.ContestantPage; }
        private ContestantPage MiniContestantPage { get => MainWindow.MiniContestantPage; }

        DispatcherTimer timer;
        bool started = false;

        public ActiveContestant()
        {
            InitializeComponent();
        }

        private void ResetTextBoxBorders(UIElementCollection control)
        {
            foreach (var child in control)
            foreach (var grandchild in (child as Grid).Children) if (grandchild is TextBox)
            {
                (grandchild as TextBox).ClearValue(Border.BorderBrushProperty);
                (grandchild as TextBox).ClearValue(Border.BorderThicknessProperty);
                (grandchild as TextBox).ClearValue(ForegroundProperty);
            }
        }

        public void LoadContestant()
        {
            MirrorWindow.Visual = MiniContestantPage;
            ResetTextBoxBorders(ScoresGrid.Children);
            ResetTextBoxBorders(ContestantPage.ScoresGrid.Children);
            ResetTextBoxBorders((MirrorWindow.Visual as ContestantPage).ScoresGrid.Children);
            (DataContext as ActiveContestantController).Score = Score;
#region init client scores
            foreach (var device in Devices.Where(d => d.Handle.Ping() && d.Enabled))
            {
                var i = device.Id;
                var soloScores = Score.SoloScores?.Where(ss => ss.Index == i);
                var lScore = 10 * soloScores?.First(ss => ss.Type == "Accuracy")?.Value ?? (Category.IsFreestyle ? 60 : 40);
                var rScore = 10 * soloScores?.First(ss => ss.Type == "Presentation")?.Value ?? (Category.IsFreestyle ? 40 : 60);
                var scores = new Scores(lScore, rScore);
                device.Handle.SendAsync(OutboundPacket.Instructions("init", setScores: scores), null);
            }
#endregion
#region refresh display screen
            void RefreshDisplay(ContestantPage page)
            {
                page.DisplayPoomsaeName.Content = Category.CategoryPoomsaes
                        .FirstOrDefault(cp => cp.Round == Category.CurrentRound && cp.Index == Score.Index)?.Poomsae.Name;
                page.DisplayTotal.Content = "";
                page.DisplayTimer.Content = "1:30";
            }
            RefreshDisplay(ContestantPage);
            RefreshDisplay(MiniContestantPage);
            RefreshDisplay(MirrorWindow.Visual as ContestantPage);
            DisplayScreen.DisplayScreenFrame.Content = ContestantPage;
            MirrorWindow.Visual = MiniContestantPage;
#endregion
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (started)
            {
                timer.Stop();
                started = false;
                Start.Content = "Start";
                CloseWindow.IsEnabled = true;
                return;
            }

            SetClientsIdle(false);
#region set timer
            var timeSpan = TimeSpan.FromSeconds(90);
            timer = new DispatcherTimer(new TimeSpan(0, 0, 1),
                DispatcherPriority.Normal,
                delegate
                {
                    ContestantPage.DisplayTimer.Content = timeSpan.ToString(@"mm\:ss");
                    MiniContestantPage.DisplayTimer.Content = timeSpan.ToString(@"mm\:ss");
                    (MirrorWindow.Visual as ContestantPage).DisplayTimer.Content = timeSpan.ToString(@"mm\:ss");
                    if (timeSpan == TimeSpan.Zero) timer.Stop();
                    timeSpan += TimeSpan.FromSeconds(-1);
                },
                Application.Current.Dispatcher);
#endregion

            timer.Start();
            started = true;
            Start.Content = "Stop";
            CloseWindow.IsEnabled = false;
        }
        private void CalculatePoints(UIElementCollection children, bool calculate = true)
        {
            ResetTextBoxBorders(children);
            var allTextBoxes = new List<TextBox>();
            var allTextBoxesL = children.Map(c => c.Visibility == Visibility.Visible, c => (c as Grid).Children[1] as TextBox);
            var allTextBoxesR = children.Map(c => c.Visibility == Visibility.Visible, c => (c as Grid).Children[2] as TextBox);

            allTextBoxes.AddRange(allTextBoxesL);
            allTextBoxes.AddRange(allTextBoxesR);
            var redCells = new List<TextBox>()
            {
                allTextBoxesL.Find(tb => double.Parse(tb.Text) == allTextBoxesL.Min(b => double.Parse(b.Text))),
                allTextBoxesR.Find(tb => double.Parse(tb.Text) == allTextBoxesR.Min(b => double.Parse(b.Text)))
            };
            allTextBoxesL.Reverse();
            allTextBoxesR.Reverse();
            redCells.Add(allTextBoxesL.Find(tb => double.Parse(tb.Text) == allTextBoxesL.Max(b => double.Parse(b.Text))));
            redCells.Add(allTextBoxesR.Find(tb => double.Parse(tb.Text) == allTextBoxesR.Max(b => double.Parse(b.Text))));
            foreach (var textBox in redCells)
            {
                textBox.BorderBrush = Brushes.Red;
                textBox.Foreground = Brushes.Gray;
            }
            if (calculate)
            {
                Score.DisAccuracyGrandTotal = allTextBoxesL.Sum(tb => double.Parse(tb.Text));
                Score.DisAccuracyMinorTotal = allTextBoxesL.Where(tb => !redCells.Contains(tb)).Sum(tb => double.Parse(tb.Text));
                Score.DisPresentationGrandTotal = allTextBoxesR.Sum(tb => double.Parse(tb.Text));
                Score.DisPresentationMinorTotal = allTextBoxesR.Where(tb => !redCells.Contains(tb)).Sum(tb => double.Parse(tb.Text));
                Score.DisGrandTotal = Score.DisAccuracyGrandTotal + Score.DisPresentationGrandTotal;
                Score.DisMinorTotal = Score.DisAccuracyMinorTotal + Score.DisPresentationMinorTotal;
                Score.DisMinorMean = Score.DisMinorTotal / 2 / (RefNo - (redCells.All(it => it != null) ? redCells.Count / 2 : 0));
                // has to be here because i haven't successfully implemented OnPropertyChanged for it yet.
                ContestantPage.DisplayTotal.Content = ((double)Score.MinorMean).ToString("0.00");
                MiniContestantPage.DisplayTotal.Content = ((double)Score.MinorMean).ToString("0.00");
                // successfully
                (DataContext as ActiveContestantController).RaisePropertyChanged("Score");

                var arr = allTextBoxes.Select(tb => tb.Text + (redCells.Contains(tb) ? "R" : "")).ToArray();
                var count = arr.Length / 2;
                var scoreString = string.Join(" ", arr, 0, count) + "::" + string.Join(" ", arr, count, count);
                WSSV.WebSocketServices["/TKD"].Sessions.BroadcastAsync(OutboundPacket.Instructions("all_scores", message: scoreString), null);
            }
        }
        private void ShowPoints_Click(object sender, RoutedEventArgs e)
        {
            CalculatePoints(ScoresGrid.Children);
            CalculatePoints(ContestantPage.ScoresGrid.Children, false);
            CalculatePoints(MiniContestantPage.ScoresGrid.Children, false);
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            SetClientsIdle(true);
            Hide();
            var soloScores = Context.SoloScores.Local.Where(ss => ss.Score == Score).ToList();
            if (Context.Entry(Score).State == EntityState.Added)
            {
                foreach (var ss in soloScores) Context.SoloScores.Local.Remove(ss);
                Context.Scores.Local.Remove(Score);
            }
            else
            {
                foreach (var ss in soloScores) Context.Entry(ss).Reload();
                Context.Entry(Score).Reload();
            }
            Context.SaveChanges();
                
            ParentWindow.UpdateContestants(Contestant.Category);
        }

        private void SetClientsIdle(bool idle)
        {
            WSSV.WebSocketServices["/TKD"].Sessions.BroadcastAsync(OutboundPacket.Instructions("idle", idle: idle), null);
        }

        private void AcceptScore_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ActiveContestantController).RaisePropertyChanged("Score");
            SetClientsIdle(true);
            ShowPoints_Click(sender, e);
            Hide();
            if (Context.Entry(Score).State == EntityState.Added)
            {
                Context.Scores.Local.Add(Score);
            }
            Context.SaveChanges();
            ParentWindow.ScoreViewSource.View.Refresh();
            ParentWindow.Round1Contestants.ItemsSource = ParentWindow.UpdateContestants(Contestant.Category);
            ParentWindow.Round2Contestants.ItemsSource = ParentWindow.PrepList(1, 2, 10);
            ParentWindow.Round3Contestants.ItemsSource = ParentWindow.PrepList(2, 3, 8);
        }

        private void Pull(object sender, RoutedEventArgs e)
        {
            var ID = int.Parse((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Button).Tag.ToString());
            Devices.First(d => d.Id == ID).Handle.Send(OutboundPacket.Instructions("scores", scores: true));
            MainWindow.LogWindow.Log("Pulled");
        }
        private void Clear(object sender, RoutedEventArgs e)
        {
            var ID = int.Parse((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Button).Tag.ToString());
            Devices.First(d => d.Id == ID).Handle.Send(OutboundPacket.Instructions("init", scores: true));
            MainWindow.LogWindow.Log("Cleared");
        }
        private void Init(object sender, RoutedEventArgs e)
        {
            var scores = Category.IsFreestyle ? new Scores(40, 60) : new Scores(60, 40);
            var ID = int.Parse((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Button).Tag.ToString());
            Devices.First(d => d.Id == ID).Handle.Send(OutboundPacket.Instructions("init", setScores: scores));
            ((ScoresGrid.Children[ID + 1] as Grid).Children[1] as TextBox).Text = (scores.Accuracy / 10f).ToString("0.0");
            ((ScoresGrid.Children[ID + 1] as Grid).Children[2] as TextBox).Text = (scores.Presentation / 10f).ToString("0.0");
            MainWindow.LogWindow.Log($"Initialised #{ID}");
        }
        private void Release(object sender, RoutedEventArgs e)
        {
            var ID = int.Parse((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Button).Tag.ToString());
            var device = Devices.First(d => d.Id == ID);
            device.Handle.Send(OutboundPacket.Instructions("disconnect"));
        }

        private readonly Regex Regex = new Regex(@"(?:^\d|^\.\d|^\d\.\d?)$");
        private void TextBoxTextChanged(object sender, EventArgs e)
        {
            var textBox = (sender as TextBox).Also(t => t.Text = t.Text.Replace(",", "."));
            var text = textBox.Text;
            if (text.Length > 0)
                textBox.CaretIndex = text.Length;
            if (!Regex.IsMatch(text))
            {
                textBox.Background = Brushes.LightSalmon;
                return;
            }
            textBox.ClearValue(BackgroundProperty);
        }
        private void TextBoxKeyPress(object sender, KeyEventArgs e)
        {
            var keyVal = new KeyConverter().ConvertToString(e.Key);
            switch (e.Key)
            {
                case Key.Tab: keyVal = "\t"; break;
                case Key.OemPeriod:
                case Key.Decimal:
                case Key.OemComma: keyVal = "."; break;
                case Key.Left:
                case Key.Right: 
                case Key.Delete:
                case Key.Back: return;
            }
            var text = (sender as TextBox).Text;
            var match = Regex.IsMatch(text);
            
            if (!Regex.IsMatch(keyVal, @"[\t\.\d]") || (keyVal == "\t" && !match))
            {
                e.Handled = true;
            }
        }

        private void NeverClose(object sender, System.ComponentModel.CancelEventArgs e) => Hide();
    }
}
