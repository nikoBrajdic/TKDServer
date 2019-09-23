using System.Windows.Controls;
using TKD.App.Controllers;
using TKD.App.Models;

namespace TKD.App.Views
{
    /// <summary>
    /// Interaction logic for ContestantPage.xaml
    /// </summary>
    public partial class ContestantPage : Page
    {
        public Performer Performer
        {
            get => (DataContext as ActiveContestantController).Performer;
            set => (DataContext as ActiveContestantController).Performer = value;
        }
        public Score Score { get => Performer.Score; }

        public ContestantPage()
        {
            InitializeComponent();
        }
    }
}
