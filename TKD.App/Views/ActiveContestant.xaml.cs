//#define MVVM

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using TKD.App.Controllers;
using TKD.App.Models;
using TKD.App.Views;
using TKD.App;
using WebSocketSharp.Server;

namespace TKD.App
{
    /// <summary>
    /// Interaction logic for ActiveContestant.xaml
    /// </summary>
    public partial class ActiveContestant : Window
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

        public ActiveContestant()
        {
            InitializeComponent();

#if MVVM
            DataContext = new DoubleTextConverter();
#endif            
        }

        DispatcherTimer timer;
        bool started = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


#if MVVM
            (DataContext as DoubleTextConverter).Score = Score; 
#endif

            DisableUnusedFields();
        }



        /// <summary>
        /// COLUMN PART NEEDS TO GO TO CONTESTANT PAGE CONTROLLER
        /// </summary>
        private void DisableUnusedFields()
        {
            for (int i = 1; i < ScoresGrid.RowDefinitions.Count; i++)
            {
                foreach (var side in new List<string> { "L", "R" })
                {
                    var scoreBox = Node<TextBox>($"{side}Score{i}", ScoresGrid);
                    var thereBox = Node<TextBox>($"{side}{i}", ContestantPage);
                    var column = Node<ColumnDefinition>($"Column{i}", ContestantPage);
                    column.Width = new GridLength(1, GridUnitType.Star);
                    scoreBox.BorderBrush = Brushes.Black;
                    scoreBox.Background = default;
                    scoreBox.IsEnabled = true;
                    thereBox.Background = default;
                    if (i > RefNo)
                    {
                        scoreBox.IsEnabled = false;
                        scoreBox.Background = Brushes.DimGray;
                        column.Width = new GridLength(0, GridUnitType.Pixel);
                    }
                }
            }
        }

        public void RefreshData()
        {

#if MVVM
            Performer.Score.Accuracy1 = (DataContext as DoubleTextConverter).Score.Accuracy1;
            Performer.Score.Presentation1 = (DataContext as DoubleTextConverter).Score.Presentation1;

#endif
#region init client scores
            foreach (var device in Devices.Where(d => d.Handle.Ping() && d.Enabled))
            {
                var i = device.Id;
                var lScore = 10*Get<double?>(typeof(Score), Performer.Score, $"Accuracy{i}") ?? (Category.IsFreestyle ? 60 : 40);
                var rScore = 10*Get<double?>(typeof(Score), Performer.Score, $"Presentation{i}") ?? (Category.IsFreestyle ? 40 : 60);
                var scores = new Scores(Convert.ToInt32(lScore), Convert.ToInt32(rScore));
                var serialised = OutboundPacket.Instructions("init", setScores: scores);
                device.Handle.SendAsync(serialised, null);
            }
#endregion

#region refresh display screen
            var currRound = Category.CurrentRound;
            var index = Score.Index;
            ContestantPage.DisplayContestantName.Content = Contestant.ToString();
            ContestantPage.DisplayTeamName.Content = Contestant.Team;
            ContestantPage.DisplayPoomsaeName.Content = Get<Poomsae>(typeof(Category), Category, $"Poomsae{currRound}{index}");
            DisplayScreen.DisplayScreenFrame.Content = ContestantPage;
            ContestantPage.DisplayTotal.Content = "";
            ContestantPage.DisplayTimer.Content = "1:30";
            ContestantName.Content = Contestant.ToString();
#endregion

#region set score fields
            for (int i = 1; i < ScoresGrid.RowDefinitions.Count; i++)
            {
#if MVVM
                if (i == 1) continue; 
#endif
                foreach (var side in new List<string> { "L", "R" })
                {
                    var hereBox = Node<TextBox>($"{side}Score{i}", ScoresGrid);
                    var thereBox = Node<TextBox>($"{side}{i}", ContestantPage);
                    var criterion = side == "L" ? "Accuracy" : "Presentation";
                    hereBox.Text = Get<double?>(typeof(Score), Score, $"{criterion}{i}")?.ToString();
                    thereBox.Text = hereBox.Text;
                }
            }
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
        private void ShowPoints_Click(object sender, RoutedEventArgs e)
        {
            bool scoresMissing = false;
            var allScoresL = new List<string>();
            var allScoresP = new List<string>();
            foreach (var item in new List<string> { "Accuracy", "Presentation" })
            {
                var left = item == "Accuracy";
                var lowestHereScore   = left ? LScore1 : RScore1;
                var highestHereScore  = left ? LScore2 : RScore2;
                var lowestThereScore  = left ? ContestantPage.L1 : ContestantPage.R1;
                var highestThereScore = left ? ContestantPage.L2 : ContestantPage.R2;
                var allScores         = left ? allScoresL : allScoresP;
                var side              = left ? "L" : "R";
                double.TryParse(lowestHereScore.Text, out double lowest);
                double.TryParse(highestHereScore.Text, out double highest);
                double currTotal = 0;
                for (int i = 1; i <= RefNo; i++)
                {
                    var thereBox = Node<TextBox>($"{side}{i}", ContestantPage);
                    var hereBox = Node<TextBox>($"{side}Score{i}");
                    thereBox.Text = double.Parse(hereBox.Text).ToString("0.0");

                    bool pass = double.TryParse(hereBox.Text, out double currScore);
                    if (pass && currScore <= lowest)
                    {
                        lowestHereScore = hereBox;
                        lowestThereScore = thereBox;
                        lowest = currScore;
                    }
                    if (pass && currScore >= highest)
                    {
                        highestHereScore = hereBox;
                        highestThereScore = thereBox;
                        highest = currScore;
                    }
                    scoresMissing = scoresMissing || !pass;
                    currTotal += currScore;
                    allScores.Add(currScore.ToString("0.0"));

                    Set(typeof(Score), Score, $"{item}{i}", currScore == 0 ? (double?)null : currScore);
                }
                Set(typeof(Score), Score, $"{item}GrandTotal", currTotal);
                Set(typeof(Score), Score, $"{item}MinorTotal", currTotal - lowest - highest);
                lowestHereScore.BorderBrush = Brushes.Red;
                highestHereScore.BorderBrush = Brushes.Red;
                lowestThereScore.Background = Brushes.Salmon;
                highestThereScore.Background = Brushes.Salmon;
                allScores[int.Parse(lowestHereScore.Tag.ToString())-1] += 'R';
                allScores[int.Parse(highestHereScore.Tag.ToString())-1] += 'R';
            }

            if (scoresMissing)
            {
                MessageBox.Show("Scores missing! Either force pull from devices or add manually!",
                                "Missing Scores",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }
            var allScoresFinal = string.Join(" ", allScoresL) + "::" + string.Join(" ", allScoresP);
            WSSV.WebSocketServices["/TKD"].Sessions.BroadcastAsync(OutboundPacket.Instructions("all_scores", message: allScoresFinal), null);
            Score.GrandTotal = Score.AccuracyGrandTotal + Score.PresentationGrandTotal;
            Score.MinorTotal = Score.AccuracyMinorTotal + Score.PresentationMinorTotal;
            Score.MinorMean = Math.Round((double)Score.MinorTotal / (RefNo > 3 ? RefNo - 2 : RefNo), 2);

            ContestantPage.DisplayTotal.Content = Score.MinorMean;

            MeanResult.Text = Score.MinorMean.ToString();
            MinorResult.Text = Score.MinorTotal.ToString();
            AMinorResult.Text = Score.AccuracyMinorTotal.ToString();
            ATotalResult.Text = Score.AccuracyGrandTotal.ToString();
            PMinorResult.Text = Score.PresentationMinorTotal.ToString();
            PTotalResult.Text = Score.PresentationGrandTotal.ToString();
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            SetClientsIdle(true);
            Hide();
            DisableUnusedFields();
            Context.Entry(Score).Reload();
        }

        private void SetClientsIdle(bool idle)
        {
            WSSV.WebSocketServices["/TKD"].Sessions.BroadcastAsync(OutboundPacket.Instructions("idle", idle: idle), null);
        }

        private void AcceptScore_Click(object sender, RoutedEventArgs e)
        {
            Context.Scores.Local.Add(Score);
                Context.SaveChangesAsync();
            ParentWindow.ScoreViewSource.View.Refresh();
            ParentWindow.Round1Contestants.ItemsSource = ParentWindow.UpdateRound1(Contestant.Category);
            ParentWindow.Round2Contestants.ItemsSource = ParentWindow.PrepList(1, 2, 10);
            ParentWindow.Round3Contestants.ItemsSource = ParentWindow.PrepList(2, 3, 8);
            SetClientsIdle(true);
            ShowPoints_Click(sender, e);
            Hide();
            DisableUnusedFields();
        }

        private void Pull(object sender, RoutedEventArgs e)
        {
            MainWindow.LogWindow.LogBox.Text += "Pulled";
            MainWindow.LogWindow.LogBox.ScrollToEnd();
            var ID = int.Parse((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Button).Tag.ToString());
            Devices.First(d => d.Id == ID).Handle.Send(OutboundPacket.Instructions("scores", scores: true));
        }
        private void Clear(object sender, RoutedEventArgs e)
        {
            MainWindow.LogWindow.LogBox.Text += "Cleared";
            MainWindow.LogWindow.LogBox.ScrollToEnd();
        }
        private void Init(object sender, RoutedEventArgs e)
        {
            var scores = Category.IsFreestyle ? new Scores(60, 40) : new Scores(40, 60);
            var ID = int.Parse((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Button).Tag.ToString());
            Devices.First(d => d.Id == ID).Handle.Send(OutboundPacket.Instructions("init", setScores: scores));
            MainWindow.LogWindow.LogBox.Text += $"Initialised #{ID}";
            MainWindow.LogWindow.LogBox.ScrollToEnd();
            Node<TextBox>($"LScore{ID}", ScoresGrid).Text = (scores.Accuracy / 10f).ToString("0.0");
            Node<TextBox>($"RScore{ID}", ScoresGrid).Text = (scores.Presentation / 10f).ToString("0.0");
        }
        private void Release(object sender, RoutedEventArgs e)
        {
            var ID = int.Parse((((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Button).Tag.ToString());
            Devices.First(d => d.Id == ID).Handle.Send(OutboundPacket.Instructions("disconnect"));
            MainWindow.LogWindow.LogBox.Text += "Released";
            MainWindow.LogWindow.LogBox.ScrollToEnd();
        }
        private T Node<T>(string name, UIElement parent = null) => (T) (object) LogicalTreeHelper.FindLogicalNode(parent ?? MainGrid, name);
        private T Get<T>(Type type, object parent, string name) => (T) type.GetProperty(name).GetValue(parent);
        private void Set(Type type, object parent, string name, object value) => type.GetProperty(name).SetValue(parent, value);
    }
}
