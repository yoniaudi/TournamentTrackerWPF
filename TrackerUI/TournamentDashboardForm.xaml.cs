using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TrackerLibrary.Models;
using TrackerLibrary;

namespace TrackerUI
{
    /// <summary>
    /// Interaction logic for TournamentDashboardForm.xaml
    /// </summary>
    public partial class TournamentDashboardForm : Window
    {
        private List<TournamentModel> tournaments = GlobalConfig.Connection.GetTournament_All();

        public TournamentDashboardForm()
        {
            InitializeComponent();
            WireUpLists();
        }

        private void WireUpLists()
        {
            loadExistingTournamentDropDown.ItemsSource = null;
            loadExistingTournamentDropDown.ItemsSource = tournaments;
            loadExistingTournamentDropDown.DisplayMemberPath = "TournamentName";
            /*loadExistingTournamentDropDown.DataSource = null;
            loadExistingTournamentDropDown.DataSource = tournaments;
            loadExistingTournamentDropDown.DisplayMember = "TournamentName";*/
        }
        /*
        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            CreateTournamentForm tournamentForm = new CreateTournamentForm();
            tournamentForm.Show();
        }

        private void loadExistingTournamentButton_Click(object sender, EventArgs e)
        {
            var model = (TournamentModel)loadExistingTournamentDropDown.SelectedItem;
            TournamentViewerForm tournamentViewerForm = new TournamentViewerForm(model);
            tournamentViewerForm.Show();
        }*/

        private void loadExistingTournamentButton_Click(object sender, RoutedEventArgs e)
        {
            var model = (TournamentModel)loadExistingTournamentDropDown.SelectedItem;
            TournamentViewerForm tournamentViewerForm = new TournamentViewerForm(model);
            tournamentViewerForm.Show();
        }

        private void createTournamentButton_Click(object sender, RoutedEventArgs e)
        {
            CreateTournamentForm tournamentForm = new CreateTournamentForm();
            tournamentForm.Show();
        }
    }
}
