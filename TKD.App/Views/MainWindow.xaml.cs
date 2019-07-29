using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TKD.App.Models;
using TKD.App.Views;
using WebSocketSharp.Server;
using FontFamily = System.Windows.Media.FontFamily;

namespace TKD.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CollectionViewSource CategoryViewSource;
        public CollectionViewSource ContestantViewSource;
        public CollectionViewSource PoomsaeViewSource;
        public CollectionViewSource PoomsaeTypeViewSource;
        public CollectionViewSource ScoreViewSource;
        public CollectionViewSource TeamViewSource;

        public ObservableCollection<Category> Categories { get => Context.Categories.Local; }
        public ObservableCollection<Contestant> Contestants { get => Context.Contestants.Local; }
        public ObservableCollection<Poomsae> Poomsaes { get => Context.Poomsaes.Local; }
        public ObservableCollection<PoomsaeType> PoomsaeTypes { get => Context.PoomsaeTypes.Local; }
        public ObservableCollection<Score> Scores { get => Context.Scores.Local; }
        public ObservableCollection<Team> Teams { get => Context.Teams.Local; }
        public ObservableCollection<Device> ConnectedDevices { get; } = new ObservableCollection<Device>();
        public static TkdModel Context { get; } = new TkdModel();
        public static ActiveContestant ActiveContestantWindow { get; set; }
        public static DisplayScreen DisplayScreen { get; } = new DisplayScreen();
        public static RankingsPage RankingsPage { get; } = new RankingsPage();
        public static ContestantPage ContestantPage { get; } = new ContestantPage();
        public static IdlePage IdlePage { get; } = new IdlePage();
        public static LogWindow LogWindow { get; } = new LogWindow();
        public static WebSocketServer WSSV { get; set; }
        public static Settings Settings { get; set; }
        public Queue<int> Referees { get; set; }
        List<string> tableNames;
        bool IsNew = false;
        string IP { get => Dns.GetHostEntry(Dns.GetHostName())
                        .AddressList
                        .FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork)
                        ?.ToString(); }


        public MainWindow()
        {
            InitializeComponent();
            CategoryViewSource    = (CollectionViewSource) FindResource("categoryViewSource");
            ContestantViewSource  = (CollectionViewSource) FindResource("contestantViewSource");
            PoomsaeViewSource     = (CollectionViewSource) FindResource("poomsaeViewSource");
            PoomsaeTypeViewSource = (CollectionViewSource) FindResource("poomsaeTypeViewSource");
            ScoreViewSource       = (CollectionViewSource) FindResource("scoreViewSource");
            TeamViewSource        = (CollectionViewSource) FindResource("teamViewSource");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Context.Categories.Load();
            Context.Contestants.Load();
            Context.Poomsaes.Load();
            Context.PoomsaeTypes.Load();
            Context.Scores.Load();
            Context.Teams.Load();

            CategoryViewSource.Source = Categories;
            ContestantViewSource.Source = Contestants;
            PoomsaeViewSource.Source = Poomsaes;
            PoomsaeTypeViewSource.Source = PoomsaeTypes;
            ScoreViewSource.Source = Scores;
            TeamViewSource.Source = Teams;

            CategoryComboBox.ItemsSource = Categories;
            PoomsaeTypeComboBox.ItemsSource = PoomsaeTypes;
            TeamComboBox.ItemsSource = Teams;
            new List<ComboBox>() {
                Poomsae11ComboBox, Poomsae12ComboBox, Poomsae21ComboBox,
                Poomsae22ComboBox, Poomsae31ComboBox, Poomsae32ComboBox
            }.ForEach(box => box.ItemsSource = Poomsaes);

            ChooseCategory.ItemsSource = Categories.Where(c => c.Contestants.Count(con => con.Active) > 0);

            tableNames = Context.Database
                .SqlQuery<string>("SELECT name FROM sys.tables ORDER BY name")
                .Where(t => !(t.Contains("MigrationsHistory") || t == "sysdiagrams"))
                .Select(n => n.EndsWith("ies") ? n.Replace("ies", "y") : n.TrimEnd('s'))
                .ToList();
            tableNames.ForEach(name => PrepButtons(false, true, true, true, false, true, false, name));

            Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("properties.json"));
            IdlePage.ContestName.Content = Settings.ContestName;
            IdlePage.Date.Content = DateTime.Today.ToString("dd. MMMM yyyy.", CultureInfo.InvariantCulture);
            SettingsGrid.DataContext = Settings;

            ActiveContestantWindow = new ActiveContestant { ParentWindow = this, DataContext = DisplayScreen };
            DisplayScreen.DisplayScreenFrame.Content = IdlePage;
            DisplayScreen.Show();

            Referees = new Queue<int>(Enumerable.Range(1, Settings.RefNo).ToList());
            BorderThickness = new Thickness(3);
            BorderBrush = Brushes.Red;

            Task.Run(() => InitServer());
        }

        #region buttons
        // ------------------------------ NEW -----------------------------------
        private void NewCategory_Click(object sender, RoutedEventArgs e) => NewEntry<Category>();
        private void NewContestant_Click(object sender, RoutedEventArgs e) => NewEntry<Contestant>();
        private void NewPoomsae_Click(object sender, RoutedEventArgs e) => NewEntry<Poomsae>();
        private void NewPoomsaeType_Click(object sender, RoutedEventArgs e) => NewEntry<PoomsaeType>();
        private void NewScore_Click(object sender, RoutedEventArgs e) => NewEntry<Score>();
        private void NewTeam_Click(object sender, RoutedEventArgs e) => NewEntry<Team>();

        private void NewEntry<T>()
        {
            IsNew = true;
            var type = typeof(T).Name;
            object @new;
            switch (type)
            {
                case "Category":
                    @new = new Category { Name = "", ShortName = "" };
                    break;
                case "Contestant":
                    @new = new Contestant { Name = "", Surname = "" };
                    break;
                case "Poomsae":
                    @new = new Poomsae { Name = "", ShortName = "", Ordinal = "" };
                    break;
                case "PoomsaeType":
                    @new = new PoomsaeType { Name = "" };
                    break;
                case "Score":
                    @new = new Score();
                    break;
                case "Team":
                    @new = new Team { Name = "" };
                    break;
                default:
                    @new = null;
                    break;
            }
            Context.Set(typeof(T)).Local.Add((T) @new);
            var viewSource = typeof(MainWindow).GetField($"{type}ViewSource").GetValue(this) as CollectionViewSource;
            viewSource.View.Refresh();
            viewSource.View.MoveCurrentTo(@new);

            InitEditFields(((Grid)LogicalTreeHelper.FindLogicalNode(MainTabControl, $"{type}EditGrid")).Children);

            ((DataGrid)LogicalTreeHelper.FindLogicalNode(MainTabControl, $"{type}DataGrid")).ScrollIntoView(@new);

            Dispatcher.BeginInvoke(new Action(() => 
                    ((TextBox)LogicalTreeHelper.FindLogicalNode(MainTabControl, $"{type}NameTextBox")).Focus()), 
                DispatcherPriority.Input);

            PrepButtons(true, false, false, false, true, false, true, type);
        }
        // ------------------------------ SAVE -----------------------------------
        private void SaveCategory_Click(object sender, RoutedEventArgs e) => SaveEntry<Category>();
        private void SaveContestant_Click(object sender, RoutedEventArgs e) => SaveEntry<Contestant>();
        private void SavePoomsae_Click(object sender, RoutedEventArgs e) => SaveEntry<Poomsae>();
        private void SavePoomsaeType_Click(object sender, RoutedEventArgs e) => SaveEntry<PoomsaeType>();
        private void SaveScore_Click(object sender, RoutedEventArgs e) => SaveEntry<Score>();
        private void SaveTeam_Click(object sender, RoutedEventArgs e) => SaveEntry<Team>();

        private void SaveEntry<T>()
        {
            try
            {
                var type = typeof(T).Name;
                var viewSource = typeof(MainWindow).GetField($"{type}ViewSource").GetValue(this) as CollectionViewSource;
                var item = (T) viewSource.View.CurrentItem;
                switch (item)
                {
                    case Category ctgr:
                        ctgr.Poomsae11ID = (Poomsae11ComboBox.SelectedItem as Poomsae)?.ID ?? null;
                        ctgr.Poomsae12ID = (Poomsae12ComboBox.SelectedItem as Poomsae)?.ID ?? null;
                        ctgr.Poomsae21ID = (Poomsae21ComboBox.SelectedItem as Poomsae)?.ID ?? null;
                        ctgr.Poomsae22ID = (Poomsae22ComboBox.SelectedItem as Poomsae)?.ID ?? null;
                        ctgr.Poomsae31ID = (Poomsae31ComboBox.SelectedItem as Poomsae)?.ID ?? null;
                        ctgr.Poomsae32ID = (Poomsae32ComboBox.SelectedItem as Poomsae)?.ID ?? null;
                        break;
                    case Contestant ctnt:
                        ctnt.TeamId = (TeamComboBox.SelectedItem as Team).ID;
                        ctnt.CategoryId = (CategoryComboBox.SelectedItem as Category).ID;
                        break;
                    case Poomsae p:
                        p.PoomsaeTypeId = (PoomsaeTypeComboBox.SelectedItem as PoomsaeType).ID;
                        break;
                    case Score s:
                        s.ContestantId = (Contestants.FirstOrDefault(c => c.ToString() == ScoreNameTextBox.Text)).ID;
                        break;
                    default:
                        break;
                }
                Context.SaveChangesAsync();
                ChooseCategory.ItemsSource = Categories.Where(c => c.Contestants.Count(con => con.Active) > 0);
                viewSource.View.Refresh();

                PrepButtons(false, true, true, true, false, true, false, type);
                Dispatcher.BeginInvoke(new Action(() =>
                        ((DataGrid)LogicalTreeHelper.FindLogicalNode(MainTabControl, $"{type}DataGrid")).Focus()),
                    DispatcherPriority.Input);
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
            IsNew = false;
        }
        
        // ------------------------------ EDIT -----------------------------------
        private void EditCategory_Click(object sender, RoutedEventArgs e) => EditEntry<Category>();
        private void EditContestant_Click(object sender, RoutedEventArgs e) => EditEntry<Contestant>();
        private void EditPoomsae_Click(object sender, RoutedEventArgs e) => EditEntry<Poomsae>();
        private void EditPoomsaeType_Click(object sender, RoutedEventArgs e) => EditEntry<PoomsaeType>();
        private void EditScore_Click(object sender, RoutedEventArgs e) => EditEntry<Score>();
        private void EditTeam_Click(object sender, RoutedEventArgs e) => EditEntry<Team>();

        private void EditEntry<T>()
        {
            var type = typeof(T).Name;
            var current = typeof(MainWindow).GetField($"{type}ViewSource").GetValue(this) as CollectionViewSource;
            if (current.View.CurrentItem != null)
            {
                PrepButtons(true, false, false, false, true, false, true, type);
                Dispatcher.BeginInvoke(new Action(() =>
                        ((TextBox)LogicalTreeHelper.FindLogicalNode(MainTabControl, $"{type}NameTextBox")).Focus()),
                    DispatcherPriority.Input);
            }
        }
        // ------------------------------ DELETE -----------------------------------
        private void DeleteCategory_Click(object sender, RoutedEventArgs e) => DeleteEntry<Category>();
        private void DeleteContestant_Click(object sender, RoutedEventArgs e) => DeleteEntry<Contestant>();
        private void DeleteScore_Click(object sender, RoutedEventArgs e) => DeleteEntry<Score>();
        private void DeleteTeam_Click(object sender, RoutedEventArgs e) => DeleteEntry<Team>();
        private void DeletePoomsae_Click(object sender, RoutedEventArgs e) => DeleteEntry<Poomsae>();
        private void DeletePoomsaeType_Click(object sender, RoutedEventArgs e) => DeleteEntry<PoomsaeType>();

        private void DeleteEntry<T>()
        {
            var type = typeof(T).Name;
            if (MessageBox.Show("Are you sure you want to delete this item?", $"Delete {type}", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
            var viewSource = typeof(MainWindow).GetField($"{type}ViewSource").GetValue(this) as CollectionViewSource;
            if (viewSource.View.CurrentItem != null)
            {
                Context.Set(typeof(T)).Local.Remove((T) viewSource.View.CurrentItem);
                Context.SaveChangesAsync();
            }
            viewSource.View.Refresh();
            Dispatcher.BeginInvoke(new Action(() =>
                    ((DataGrid)LogicalTreeHelper.FindLogicalNode(MainTabControl, $"{type}DataGrid")).Focus()),
                DispatcherPriority.Input);
        }
        // ------------------------------ CANCEL -----------------------------------
        private void CancelCategory_Click(object sender, RoutedEventArgs e) => CancelEntry<Category>();
        private void CancelContestant_Click(object sender, RoutedEventArgs e) => CancelEntry<Contestant>();
        private void CancelPoomsae_Click(object sender, RoutedEventArgs e) => CancelEntry<Poomsae>();
        private void CancelPoomsaeType_Click(object sender, RoutedEventArgs e) => CancelEntry<PoomsaeType>();
        private void CancelScore_Click(object sender, RoutedEventArgs e) => CancelEntry<Score>();
        private void CancelTeam_Click(object sender, RoutedEventArgs e) => CancelEntry<Team>();
        private void CancelEntry<T>()
        {
            var type = typeof(T).Name;
            var viewSource = typeof(MainWindow).GetField($"{type}ViewSource").GetValue(this) as CollectionViewSource;
            var current = (T)viewSource.View.CurrentItem;
            if (IsNew)
            {
                Context.Set(typeof(T)).Remove(current);
                IsNew = false;
            }
            else
            {
                Context.Entry(current).Reload();
            }
            viewSource.View.Refresh();

            PrepButtons(false, true, true, true, false, true, false, type);
            Dispatcher.BeginInvoke(new Action(() =>
                    ((DataGrid)LogicalTreeHelper.FindLogicalNode(MainTabControl, $"{type}DataGrid")).Focus()),
                DispatcherPriority.Input);
        }
        private void PrepButtons(bool editGrid, bool dataGrid, bool newBtn, bool editBtn, bool saveBtn, bool deleteBtn, bool cancelBtn, string name)
        {
            Node<Grid>($"{name}EditGrid").IsEnabled = editGrid;
            Node<DataGrid>($"{name}DataGrid").IsEnabled = dataGrid;
            Node<Button>($"New{name}").IsEnabled = newBtn;
            Node<Button>($"Edit{name}").IsEnabled = editBtn;
            Node<Button>($"Save{name}").IsEnabled = saveBtn;
            Node<Button>($"Delete{name}").IsEnabled = deleteBtn;
            Node<Button>($"Cancel{name}").IsEnabled = cancelBtn;
        }
        // -------------------------------------------------------------------------
        #endregion

        void GridUnloaded(object sender, RoutedEventArgs e) => ((DataGrid)sender).CommitEdit(DataGridEditingUnit.Row, true);

        private void CategoryChosen(object sender, SelectionChangedEventArgs e)
        {
            TabRound1.IsEnabled = true;
            TabRound2.IsEnabled = true;
            TabRound3.IsEnabled = true;

            var RefNo = int.Parse(RefereeCount.Text);
            var chosenCategory = ChooseCategory.SelectedItem as Category;
            Round1Contestants.ItemsSource = UpdateRound1(chosenCategory);
            Round2Contestants.ItemsSource = PrepList(1, 2, 10);
            Round3Contestants.ItemsSource = PrepList(2, 3, 8);
            for (int i = 1; i <= 3; i++)
            {
                var thisRoundGrid = Node<DataGrid>($"Round{i}Contestants");
                for (int j = 1; j <= 9; j++)
                {
                    ((DataGridTextColumn)thisRoundGrid.FindName($"Round{i}Accuracy{j}")).Visibility = Visibility.Visible;
                    ((DataGridTextColumn)thisRoundGrid.FindName($"Round{i}Presentation{j}")).Visibility = Visibility.Visible;
                    if (j > RefNo)
                    {
                        ((DataGridTextColumn)thisRoundGrid.FindName($"Round{i}Accuracy{j}")).Visibility = Visibility.Hidden;
                        ((DataGridTextColumn)thisRoundGrid.FindName($"Round{i}Presentation{j}")).Visibility = Visibility.Hidden;
                    }
                }
            }
            Round1Contestants.Visibility = Visibility.Visible;
            Node<TabItem>($"TabRound{chosenCategory.CurrentRound}").Focus();
        }

        void RoundTabGotFocus(object sender, RoutedEventArgs e)
        {
            var category = ChooseCategory.SelectedItem as Category;
            category.CurrentRound = int.Parse(((TabItem)sender).Name.Replace("TabRound", ""));
            Context.SaveChanges();
            CategoryViewSource.View.Refresh();

        }

        public IEnumerable<Performer> UpdateRound1(Category chosenCategory) =>
            from c in Contestants
                join s in Scores on c equals s.Contestant into tmp
            from filteredScore in tmp.DefaultIfEmpty(new Score(c.ID, chosenCategory.CurrentRound))
                where c.Active && c.Category == chosenCategory && filteredScore.Round == 1
                select new Performer
                {
                    Contestant = c,
                    Score = filteredScore
                };

        private void OpenContestant(object sender, MouseButtonEventArgs e)
        {
            var chosenCategory = ChooseCategory.SelectedItem as Category;
            var performer = (e.Source as DataGrid).CurrentItem as Performer;
            ActiveContestantWindow.Performer = performer;
            if (chosenCategory.CurrentRound == 3)
            {
                var idx = (bool)new PoomsaeChoice().ShowDialog() ? 1 : 2;
                performer.Score = Scores.FirstOrDefault(s => 
                    s.Contestant == performer.Contestant &&
                    s.Round      == chosenCategory.CurrentRound &&
                    s.Index      == idx
                ) ?? new Score(performer.Contestant.ID, chosenCategory.CurrentRound);
            }
            WSSV.WebSocketServices["/TKD"].Sessions.BroadcastAsync(OutboundPacket.Instructions("idle", idle: false), null);
            ActiveContestantWindow.Show();
            ActiveContestantWindow.RefreshData();
        }

        private void NoPoomsae(object sender, RoutedEventArgs e)
        {
            var cbx = ((Button)sender).Tag as string;
            Node<ComboBox>(cbx).SelectedItem = null;
        }

        public IEnumerable<Performer> PrepList(int currRound, int nextRound, int quantity) =>
            Node<DataGrid>($"Round{currRound}Contestants").ItemsSource
                .Cast<Performer>()
                .OrderByDescending(p => p.Score.MinorMean)
                .ThenByDescending(p => p.Score.GrandTotal)
                .ThenByDescending(p => p.Score.MinorTotal)
                .ThenByDescending(p => p.Score.AccuracyMinorTotal)
                .ThenByDescending(p => p.Score.AccuracyGrandTotal)
                .ThenByDescending(p => p.Score.PresentationMinorTotal)
                .ThenByDescending(p => p.Score.PresentationGrandTotal)
                .Take(quantity)
                .Reverse()
                .Select(p => {
                    p.Score = Scores.FirstOrDefault(s =>
                                    s.ContestantId == p.Contestant.ID && s.Round == nextRound)
                              ?? new Score(p.Contestant.ID, nextRound);
                    return p;
                });

        private void ToNextRound(object sender, RoutedEventArgs e)
        {
            var curr = (ChooseCategory.SelectedItem as Category).CurrentRound;

            if (curr == 3)
            {
                MessageBox.Show("No further rounds.");
                return;
            }
            Node<TabItem>($"Round{curr + 1}").Focus();
        }

        private void ToPreviousRound(object sender, RoutedEventArgs e)
        {
            var curr = (ChooseCategory.SelectedItem as Category).CurrentRound;
            if (curr == 1)
            {
                MessageBox.Show("This is the first round.");
                return;
            }
            Node<TabItem>($"Round{curr - 1}").Focus();
        }

        private void ShowRankings(object sender, RoutedEventArgs e)
        {
            var curr = int.Parse((string)((sender as MenuItem).Parent as ContextMenu).Tag);
            var thisGrid = Node<DataGrid>($"Round{curr}Contestants");
            var rankings = thisGrid.ItemsSource.Cast<Performer>()
                .OrderByDescending(p => p.Score.MinorMean)
                .ThenByDescending(p => p.Score.GrandTotal)
                .ThenByDescending(p => p.Score.MinorTotal)
                .ThenByDescending(p => p.Score.AccuracyMinorTotal)
                .ThenByDescending(p => p.Score.AccuracyGrandTotal)
                .ThenByDescending(p => p.Score.PresentationMinorTotal)
                .ThenByDescending(p => p.Score.PresentationGrandTotal)
                .Select(p => p.Contestant.ToString() + "|" + p.Contestant.Team.Name)
                .ToList();
            for (int i = 0; i < rankings.Count; i++)
            {
                var contAndTeam = rankings[i].Split('|');
                rankings[i] = ((i+1) + ".").PadRight(4) + contAndTeam[0].PadRight(20) + contAndTeam[1];
            }
            var listView = new ListView
            {
                FontFamily = new FontFamily("Operator Mono SSM"),
                BorderBrush = Brushes.PowderBlue,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.PowderBlue
            };
            for (int i = 0; i < rankings.Count; i++)
            {
                var listViewItem = new ListViewItem
                {
                    Content = rankings[i],
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                switch (i)
                {
                    case 0: listViewItem.Background = Brushes.Gold; listViewItem.FontSize += 6; break;
                    case 1: listViewItem.Background = Brushes.Silver; listViewItem.FontSize += 4; break;
                    case 2: listViewItem.Background = Brushes.DarkGoldenrod; listViewItem.FontSize += 2; break;
                    default: break;
                }
                listView.Items.Add(listViewItem);
            }
            RankingsPage.RankingsViewBox.Child = listView;
            DisplayScreen.DisplayScreenFrame.Content = RankingsPage;
        }
        private void OpenLog(object sender, RoutedEventArgs e) => LogWindow.Show();

        private void Window_Closed(object sender, EventArgs e)
        {
            Task.Run(() => {
                //SetIp(dynamic: true);
                File.WriteAllText("properties.json", JsonConvert.SerializeObject(Settings, Formatting.Indented));
            });
            ActiveContestantWindow.Close();
            DisplayScreen.Close();
            LogWindow.Close();
            Close();
            Application.Current.Shutdown();
        }

        private void InitEditFields(UIElementCollection children)
        {
            foreach (var child in children)
            {
                switch (child)
                {
                    case TextBox tb: tb.Text = ""; break;
                    case ComboBox cb: cb.SelectedItem = null; break;
                    // if it's the Active Contestant check box set to checked, for everything else uncheck.
                    case CheckBox chb: chb.IsChecked = chb.Name == "ActiveCheckBox"; break;
                }
            }
        }

        private void InitServer()
        {
            WSSV?.Stop();
            //SetIp();
            WSSV = new WebSocketServer($"ws://{Settings.IP}:8088");

            WSSV.AddWebSocketService("/TKD",
                () => new TKDClient
                {
                    BaseWindow = this,
                    LogWindow = LogWindow,
                    ActiveContestantWindow = ActiveContestantWindow,
                    Devices = ConnectedDevices,
                    Referees = Referees
                }
            );
            WSSV.AddWebSocketService("/HeadRef",
                () => new TKDClient
                {
                    BaseWindow = this,
                    LogWindow = LogWindow,
                    ActiveContestantWindow = ActiveContestantWindow,
                    Devices = ConnectedDevices,
                    Referees = Referees
                }
            );
            WSSV.Start();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                BorderThickness = new Thickness(0);
                BorderBrush = Brushes.Transparent;
            }));
        }

        private T Node<T>(string name, UIElement parent = null) => (T)(object)LogicalTreeHelper.FindLogicalNode(parent ?? MainTabControl, name);

        private void HideDisplayScreen(object sender, RoutedEventArgs e)
        {
            if (DisplayScreen.Visibility == Visibility.Visible) DisplayScreen.Hide(); else DisplayScreen.Show();
        }

        private void ShowIdlePage(object sender, RoutedEventArgs e)
        {
            DisplayScreen.DisplayScreenFrame.Content = IdlePage;
        }

        private void ApplyConfigChangesButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => InitServer());
        }
        private void SetIp(bool dynamic = false)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), (dynamic ? "SetDynamic.ps1" : "SetStatic.ps1"));
            var newProcessInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                Verb = "runas",
                FileName = @"C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe",
                Arguments = $"\"&'{path}'\" -IpAddress {Settings.IP}",
                CreateNoWindow = true
            };
            var proc = new Process { StartInfo = newProcessInfo };
            proc.Start();
            while (IP != Settings.IP) Thread.Sleep(100);
            proc.WaitForExit();
        }
    }
}
