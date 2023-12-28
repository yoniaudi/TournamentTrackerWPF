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
using MessageBox = System.Windows.MessageBox;

namespace TrackerUI
{
    /// <summary>
    /// Interaction logic for CreateTournamentFrom.xaml
    /// </summary>
    public partial class CreateTournamentForm : Window, IPrizeRequester, ITeamRequester
    {
        private List<TeamModel> availableTeams = GlobalConfig.Connection.GetTeam_All();
        private List<TeamModel> selectedTeams = new();
        private List<PrizeModel> selectedPrizes = new();
        public CreateTournamentForm()
        {
            InitializeComponent();
            WireUpLists();
        }

        public void WireUpLists()
        {
            selectTeamDropDown.ItemsSource = null;
            selectTeamDropDown.ItemsSource = availableTeams;
            selectTeamDropDown.DisplayMemberPath = "TeamName";

            tournamentTeamsListBox.ItemsSource = null;
            tournamentTeamsListBox.ItemsSource = selectedTeams;
            tournamentTeamsListBox.DisplayMemberPath = "TeamName";

            prizesListBox.ItemsSource = null;
            prizesListBox.ItemsSource = selectedPrizes;
            prizesListBox.DisplayMemberPath = "PlaceName";

            /*selectTeamDropDown.DataSource = null;
            selectTeamDropDown.DataSource = availableTeams;
            selectTeamDropDown.DisplayMember = "TeamName";

            tournamentTeamsListBox.DataSource = null;
            tournamentTeamsListBox.DataSource = selectedTeams;
            tournamentTeamsListBox.DisplayMember = "TeamName";

            prizesListBox.DataSource = null;
            prizesListBox.DataSource = selectedPrizes;
            prizesListBox.DisplayMember = "PlaceName";*/
        }

        public void PrizeComplete(PrizeModel prize)
        {
            selectedPrizes.Add(prize);
            WireUpLists();
        }

        public void TeamComplete(TeamModel team)
        {
            selectedTeams.Add(team);
            WireUpLists();
        }

        private void addTeamButton_Click(object sender, RoutedEventArgs e)
        {
            var team = (TeamModel)selectTeamDropDown.SelectedItem;

            if (team == null) return;

            selectedTeams.Add(team);
            availableTeams.Remove(team);

            WireUpLists();
        }

        private void removeSelectedTeamButton_Click(object sender, RoutedEventArgs e)
        {
            var team = (TeamModel)tournamentTeamsListBox.SelectedItem;

            if (team == null) return;

            availableTeams.Add(team);
            selectedTeams.Remove(team);

            WireUpLists();
        }

        private void createPrizeButton_Click(object sender, RoutedEventArgs e)
        {
            var createPrizeForm = new CreatePrizeForm(this);
            createPrizeForm.Show();
        }

        private void removeSelectedPrizeButton_Click(object sender, RoutedEventArgs e)
        {
            var prize = (PrizeModel)prizesListBox.SelectedItem;

            if (prize == null) return;

            selectedPrizes.Remove(prize);
            WireUpLists();
        }

        private void createTournamentButton_Click(object sender, RoutedEventArgs e)
        {
            var isFeeValid = decimal.TryParse(entryFeeValue.Text, out var fee);

            if (!isFeeValid)
            {
                MessageBox.Show("You need to enter a valid Entry Fee.",
                    "Invalid Fee",
                    (MessageBoxButton)MessageBoxButtons.OK,
                    (MessageBoxImage)MessageBoxIcon.Error);
                return;
            }

            var tournament = new TournamentModel()
            {
                TournamentName = tournamentNameValue.Text,
                EntryFee = fee,
                EnteredTeams = selectedTeams,
                Prizes = selectedPrizes
            };

            TournamentLogic.CreateRounds(tournament);
            GlobalConfig.Connection.CreateTournament(tournament);

            //tournament.AlertUsersToNewRound();

            var tournamentForm = new TournamentViewerForm(tournament);
            tournamentForm.Show();
            this.Close();
        }

        private void createNewTeamLinkLabel_LinkClicked(object sender, RoutedEventArgs e)
        {
            var createTeamForm = new CreateTeamForm(this);
            createTeamForm.Show();
        }
    }
}
