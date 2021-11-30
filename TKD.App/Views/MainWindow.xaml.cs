using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
using TKD.App.Controllers;
using TKD.App.Models;
using TKD.App.Views;
using WebSocketSharp.Server;
using FontFamily = System.Windows.Media.FontFamily;
using static System.Linq.Enumerable;
using System.Text.RegularExpressions;
using ExtensionsNamespace;

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
        public static TkdModel Context { get; private set; }
        public static ActiveContestantController ActiveContestantController { get; } = new ActiveContestantController();
        public static ActiveContestant ActiveContestantWindow { get; } = new ActiveContestant();
        public static Audience Audience { get; } = new Audience();
        public static RankingsPage RankingsPage { get; } = new RankingsPage();
        public static ContestantPage ContestantPage { get; } = new ContestantPage();
        public static ContestantPage MiniContestantPage { get; } = new ContestantPage();
        public static IdlePage IdlePage { get; } = new IdlePage();
        public static LogWindow LogWindow { get; } = new LogWindow();
        public static WebSocketServer WSSV { get; set; }
        public static Settings Settings { get; set; }
        public Queue<int> Referees { get; set; }
        string IP
        {
            get => Dns.GetHostEntry(Dns.GetHostName())
                        .AddressList
                        .FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork)
                        ?.ToString();
        }


        public MainWindow()
        {
            InitializeComponent();
            CategoryViewSource    = (CollectionViewSource) FindResource("categoryViewSource");
            ContestantViewSource  = (CollectionViewSource) FindResource("contestantViewSource");
            PoomsaeViewSource     = (CollectionViewSource) FindResource("poomsaeViewSource");
            PoomsaeTypeViewSource = (CollectionViewSource) FindResource("poomsaeTypeViewSource");
            ScoreViewSource       = (CollectionViewSource) FindResource("scoreViewSource");
            TeamViewSource        = (CollectionViewSource) FindResource("teamViewSource");

            Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("properties.json"));
            Context = new TkdModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Context.Categories.Load();
            Context.CategoryPoomsaes.Load();
            Context.Contestants.Load();
            Context.Poomsaes.Load();
            Context.PoomsaeTypes.Load();
            Context.Scores.Load();
            Context.SoloScores.Load();
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

            Context.Database
                .SqlQuery<string>("SELECT name FROM sys.tables ORDER BY name")
                .Where(t => !(t.StartsWith("__") || t == "sysdiagrams" || t == "SoloScores" || t == "CategoryPoomsaes"))
                .Select(n => n.EndsWith("ies") ? n.Replace("ies", "y") : n.TrimEnd('s'))
                .ToList()
                .ForEach(name => PrepButtons(false, true, true, true, false, true, false, name));

            IdlePage.DataContext = Settings;
            SettingsGrid.DataContext = Settings;
            ActiveContestantWindow.ParentWindow = this;
            ActiveContestantWindow.DataContext = ActiveContestantController;
            ContestantPage.DataContext = ActiveContestantController;
            MiniContestantPage.DataContext = ActiveContestantController;
            Audience.DisplayScreenFrame.Content = IdlePage;

            Referees = new Queue<int>(Range(1, Settings.RefNo));
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
            var type = typeof(T).Name;
            object @new = type switch
            {
                "Category"    => new Category { Name = "", ShortName = "" },
                "Contestant"  => new Contestant { Name = "", Surname = "" },
                "Poomsae"     => new Poomsae { Name = "", ShortName = "", Ordinal = "" },
                "PoomsaeType" => new PoomsaeType { Name = "" },
                "Score"       => new Score(),
                "Team"        => new Team { Name = "" },
                _             => null
            };
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
                        Context.CategoryPoomsaes.AddRange(new CategoryPoomsae[]
                        {
                            new CategoryPoomsae { Round = 1, Index = 1, Category = ctgr, Poomsae = Poomsae11ComboBox.SelectedItem as Poomsae },
                            new CategoryPoomsae { Round = 1, Index = 2, Category = ctgr, Poomsae = Poomsae12ComboBox.SelectedItem as Poomsae },
                            new CategoryPoomsae { Round = 2, Index = 1, Category = ctgr, Poomsae = Poomsae21ComboBox.SelectedItem as Poomsae },
                            new CategoryPoomsae { Round = 2, Index = 2, Category = ctgr, Poomsae = Poomsae22ComboBox.SelectedItem as Poomsae },
                            new CategoryPoomsae { Round = 3, Index = 1, Category = ctgr, Poomsae = Poomsae31ComboBox.SelectedItem as Poomsae },
                            new CategoryPoomsae { Round = 3, Index = 2, Category = ctgr, Poomsae = Poomsae32ComboBox.SelectedItem as Poomsae }
                        });
                        break;
                    case Contestant ctnt:
                        ctnt.Team = TeamComboBox.SelectedItem as Team;
                        ctnt.Category = CategoryComboBox.SelectedItem as Category;
                        break;
                    case Poomsae p:
                        p.PoomsaeType = PoomsaeTypeComboBox.SelectedItem as PoomsaeType;
                        break;
                    case Score s:
                        s.Contestant = Contestants.FirstOrDefault(c => c.FullName == ScoreNameTextBox.Text);
                        break;
                    default:
                        break;
                }
                Context.SaveChanges();
                ChooseCategory.ItemsSource = Categories.Where(c => c.Contestants.Count(con => con.Active) > 0);
                viewSource.View.Refresh();
                switch (item)
                {
                    case Contestant ctnt: UpdateCategory(ctnt.Category); break;
                    case Score s: UpdateCategory(s.Contestant.Category); break;
                }
                UpdateCategory(ChooseCategory.SelectedItem as Category);
                PrepButtons(false, true, true, true, false, true, false, type);
                Dispatcher.BeginInvoke(new Action(() =>
                        ((DataGrid)LogicalTreeHelper.FindLogicalNode(MainTabControl, $"{type}DataGrid")).Focus()),
                    DispatcherPriority.Input);
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
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
                if (type == "Score")
                {
                    Context.SoloScores.RemoveRange(Context.Entry(viewSource.View.CurrentItem as Score).Entity.SoloScores);
                }
                if (type == "Category")
                {
                    Context.CategoryPoomsaes.RemoveRange(Context.Entry(viewSource.View.CurrentItem as Category).Entity.CategoryPoomsaes);
                }
                Context.Set(typeof(T)).Local.Remove((T) viewSource.View.CurrentItem);
                Context.SaveChanges();
            }
            viewSource.View.Refresh();
            UpdateCategory(ChooseCategory.SelectedItem as Category);
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
            if (Context.Entry(current).State == EntityState.Added)
            {
                Context.Set(typeof(T)).Remove(current);
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

            var chosenCategory = ChooseCategory.SelectedItem as Category;
            UpdateCategory(chosenCategory);

            foreach (var grid in new List<DataGrid>() { Round1Contestants, Round2Contestants, Round3Contestants })
            foreach (var column in grid.Columns)
            if (new Regex($@"^[AP][1-{Settings.RefNo}]$").IsMatch(column.Header.ToString()))
            {
                column.Visibility = Visibility.Visible;
            }

            Round1Contestants.Visibility = Visibility.Visible;
            Node<TabItem>($"TabRound{chosenCategory.CurrentRound}").Focus();
        }

        void UpdateCategory(Category cat)
        {
            Round1Contestants.ItemsSource = UpdateContestants(cat);
            Round2Contestants.ItemsSource = PrepList(1, 2, 10);
            Round3Contestants.ItemsSource = PrepList(2, 3, 8);

        }

        void RoundTabGotFocus(object sender, RoutedEventArgs e)
        {
            var category = ChooseCategory.SelectedItem as Category;
            category.CurrentRound = int.Parse(((TabItem)sender).Name.Replace("TabRound", ""));
            Context.SaveChanges();
            CategoryViewSource.View.Refresh();

        }

        public IEnumerable<Performer> UpdateContestants(Category chosenCategory) =>
            from c in Contestants
                join s in Scores on c equals s.Contestant into tmp
            from filteredScore in tmp.DefaultIfEmpty(new Score(c, chosenCategory.CurrentRound))
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

            if (chosenCategory.CurrentRound == 3)
            {
                var idx = (bool)new PoomsaeChoice().ShowDialog() ? 1 : 2;
                performer.Score = Scores.FirstOrDefault(s =>
                    s.Contestant == performer.Contestant &&
                    s.Round      == chosenCategory.CurrentRound &&
                    s.Index      == idx
                ) ?? new Score(performer.Contestant, chosenCategory.CurrentRound);
                performer.Score.Index = idx;
            }

            if (performer.Score.SoloScores == null || performer.Score.SoloScores?.Count == 0)
            foreach (var _ in Range(0, Referees.Count))
            foreach (var type in new List<string> { "Accuracy", "Presentation" })
                Context.SoloScores.Local.Add(new SoloScore
                {
                    Index = chosenCategory.CurrentRound,
                    Score = performer.Score,
                    Value = 0,
                    Type = type
                });

            ActiveContestantController.Performer = performer;

            //WSSV.WebSocketServices["/TKD"].Sessions.BroadcastAsync(OutboundPacket.Instructions("idle", idle: false), null);
            ActiveContestantWindow.Show();
            ActiveContestantWindow.LoadContestant();
            //ContestantPage.ScoresGrid.Visibility = Visibility.Hidden;
        }

        private void NoPoomsae(object sender, RoutedEventArgs e)
        {
            var cbx = ((Button)sender).Tag as string;
            CategoryEditGrid.Children.Apply(c => c is ComboBox && (c as ComboBox).Name == cbx, c => (c as ComboBox).SelectedItem = null);
            //Node<ComboBox>(cbx).SelectedItem = null;
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
                    p.Score = Scores.FirstOrDefault(s => s.Contestant == p.Contestant && s.Round == nextRound)
                              ?? new Score(p.Contestant, nextRound);
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
            MainTabControl.Items.Apply(ti => (ti as TabItem).Name == $"Round{curr + 1}", ti => (ti as TabItem).Focus());
            //Node<TabItem>($"Round{curr + 1}").Focus();
        }

        private void ToPreviousRound(object sender, RoutedEventArgs e)
        {
            var curr = (ChooseCategory.SelectedItem as Category).CurrentRound;
            if (curr == 1)
            {
                MessageBox.Show("This is the first round.");
                return;
            }
            MainTabControl.Items.Apply(ti => (ti as TabItem).Name == $"Round{curr - 1}", ti => (ti as TabItem).Focus());
            //Node<TabItem>($"Round{curr - 1}").Focus();
        }

        private void ShowRankings(object sender, RoutedEventArgs e)
        {
            var rankings = (((sender as MenuItem)
                .Parent as ContextMenu)
                .PlacementTarget as DataGrid)
                .ItemsSource
                .Cast<Performer>()
                .OrderByDescending(p => p.Score.MinorMean)
                .ThenByDescending(p => p.Score.GrandTotal)
                .ThenByDescending(p => p.Score.MinorTotal)
                .ThenByDescending(p => p.Score.AccuracyMinorTotal)
                .ThenByDescending(p => p.Score.AccuracyGrandTotal)
                .ThenByDescending(p => p.Score.PresentationMinorTotal)
                .ThenByDescending(p => p.Score.PresentationGrandTotal)
                .Select(p => (p.Contestant.FullName, p.Contestant.Team.Name))
                .ToList();
            var miniView = new ListView();
            var listView = new ListView();
            foreach (var lv in new List<ListView>() { listView, miniView })
            {
                lv.FontFamily = new FontFamily("Operator Mono SSM");
                lv.BorderBrush = Brushes.PowderBlue;
                lv.BorderThickness = new Thickness(0);
                lv.HorizontalAlignment = HorizontalAlignment.Center;
                lv.VerticalAlignment = VerticalAlignment.Center;
                lv.Background = Brushes.PowderBlue;
                rankings.Select(ct => new ListViewItem
                {
                    Content = (rankings.IndexOf(ct) + 1 + ".").PadRight(4) + ct.FullName.PadRight(20) + ct.Name,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                }).ToList().ForEach(lvi => lv.Items.Add(lvi));

                (lv.Items[0] as ListViewItem).Background = Brushes.Gold; (lv.Items[0] as ListViewItem).FontSize += 6;
                (lv.Items[1] as ListViewItem).Background = Brushes.Silver; (lv.Items[1] as ListViewItem).FontSize += 4;
                (lv.Items[2] as ListViewItem).Background = Brushes.DarkGoldenrod; (lv.Items[2] as ListViewItem).FontSize += 2;
            }
            RankingsPage.RankingsViewBox.Child = listView;
            Audience.DisplayScreenFrame.Content = RankingsPage;
            var miniRankingsPage = new RankingsPage();
            miniRankingsPage.RankingsViewBox.Child = miniView;
            ActiveContestantWindow.MirrorWindow.Visual = miniRankingsPage;
        }
        private void OpenLog(object sender, RoutedEventArgs e) => LogWindow.Show();

        private void Window_Closed(object sender, EventArgs e)
        {
            Task.Run(() => {
                //SetIp(dynamic: true);
                File.WriteAllText("properties.json", JsonConvert.SerializeObject(Settings, Formatting.Indented));
            });
            ActiveContestantWindow.Close();
            Audience.Close();
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
            //WSSV = new WebSocketServer($"ws://{Settings.IP}:8088");

            //foreach (var name in new List<string>() { "/TKD", "/HeadRef" })
            /*WSSV.AddWebSocketService(name,
                () => new TKDClient
                {
                    BaseWindow = this,
                    LogWindow = LogWindow,
                    ActiveContestantWindow = ActiveContestantWindow,
                    Devices = ConnectedDevices,
                    Referees = Referees,
                    AppContext = Context,
                    ActiveContestantController = ActiveContestantController
                }
            );*/
            //WSSV.Start();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                BorderThickness = new Thickness(0);
                BorderBrush = Brushes.Transparent;
            }));
        }

        private T Node<T>(string name, UIElement parent = null) => (T)(object)LogicalTreeHelper.FindLogicalNode(parent ?? MainTabControl, name);

        private void ShowHideDisplayScreen(object sender, RoutedEventArgs e)
        {
            if (Audience.Visibility == Visibility.Visible) Audience.Hide(); else Audience.Show();
        }

        private void ShowIdlePage(object sender, RoutedEventArgs e)
        {
            Audience.DisplayScreenFrame.Content = IdlePage;
            ActiveContestantWindow.MirrorWindow.Visual = new IdlePage() { DataContext = Settings };
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
            using (var proc = new Process { StartInfo = newProcessInfo })
            {
                proc.Start();
                while (IP != Settings.IP) Thread.Sleep(100);
                proc.WaitForExit();
            }
        }
    }
}
