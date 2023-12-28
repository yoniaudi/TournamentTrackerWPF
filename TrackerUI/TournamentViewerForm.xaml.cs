using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrackerLibrary;
using TrackerLibrary.Models;
using MessageBox = System.Windows.MessageBox;

namespace TrackerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TournamentViewerForm : Window
    {
        private TournamentModel tournament;
        private BindingList<int> rounds = new();
        private BindingList<MatchupModel> selectedMatchups = new();

        public TournamentViewerForm(TournamentModel model)
        {
            InitializeComponent();

            tournament = model;
            tournament.OnTournamentComplete += Tournament_OnTournamentComplete;

            WireUpLists();
            LoadFormData();
            LoadRounds();
        }

        private void Tournament_OnTournamentComplete(object? sender, DateTime e)
        {
            this.Close();
        }

        private void WireUpLists()
        {
            roundDropDown.ItemsSource = rounds;
            matchupListBox.ItemsSource = selectedMatchups;
            matchupListBox.DisplayMemberPath = "DisplayName";
        }

        private void LoadFormData()
        {
            tournamentName.Content = tournament.TournamentName;
        }

        private void LoadRounds()
        {
            rounds.Clear();
            rounds.Add(1);

            var currRound = 1;

            foreach (var matchups in tournament.Rounds)
            {
                if (matchups.First().MatchupRound > currRound)
                {
                    currRound = matchups.First().MatchupRound;
                    rounds.Add(currRound);
                }
            }

            LoadMatchups(1);
        }

        private void roundDropDown_SelectionChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private void LoadMatchups(int round)
        {
            foreach (var matchups in tournament.Rounds)
            {
                if (matchups.First().MatchupRound == round)
                {
                    selectedMatchups.Clear();
                    foreach (var matchup in matchups)
                    {
                        if (matchup.Winner == null || !unplayedOnlyCheckbox.IsChecked.Value)
                        {
                            selectedMatchups.Add(matchup);
                        }
                    }
                }
            }

            if (selectedMatchups.Count > 0)
            {
                LoadMatchup(selectedMatchups.First());
            }

            DisplayMatchupInfo();
        }

        private void DisplayMatchupInfo()
        {
            var isVisible = (selectedMatchups.Count > 0) ? Visibility.Visible : Visibility.Collapsed;


            teamOneName.Visibility = isVisible;
            teamOneScoreLabel.Visibility = isVisible;
            teamOneScoreValue.Visibility = isVisible;

            teamTwoName.Visibility = isVisible;
            teamTwoScoreLabel.Visibility = isVisible;
            teamTwoScoreValue.Visibility = isVisible;

            versusLabel.Visibility = isVisible;
            scoreButton.Visibility = isVisible;
        }

        private void LoadMatchup(MatchupModel model)
        {
            if (model == null) return;

            var unsetTeamName = "Not yet set";

            for (var i = 0; i < model.Entries.Count; ++i)
            {
                if (i == 0)
                {
                    if (model.Entries[0].TeamCompeting != null)
                    {
                        teamOneName.Content = model.Entries[0].TeamCompeting.TeamName;
                        teamOneScoreValue.Text = model.Entries[0].Score.ToString();

                        teamTwoName.Content = "<bye>";
                        teamTwoScoreValue.Text = "0";
                    }
                    else
                    {
                        teamOneName.Content = unsetTeamName;
                        teamOneScoreValue.Text = "0";
                    }
                }

                if (i == 1)
                {
                    if (model.Entries[1].TeamCompeting != null)
                    {
                        teamTwoName.Content = model.Entries[1].TeamCompeting.TeamName;
                        teamTwoScoreValue.Text = model.Entries[1].Score.ToString();
                    }
                    else
                    {
                        teamTwoName.Content = unsetTeamName;
                        teamTwoScoreValue.Text = "0";
                    }
                }
            }
        }

        private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchup((MatchupModel)matchupListBox.SelectedItem);
        }

        private void unplayedOnlyCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private string ValidateData()
        {
            var output = "";
            var isTeamOneScoreValid = double.TryParse(teamOneScoreValue.Text, out var teamOneScore);
            var isTeamTwoScoreValid = double.TryParse(teamTwoScoreValue.Text, out var teamTwoScore);

            teamOneScoreValue.Text = teamOneScore.ToString();
            teamTwoScoreValue.Text = teamTwoScore.ToString();

            if (!isTeamOneScoreValid)
            {
                output = "Please enter a valid score value for team 1.";
            }
            else if (!isTeamTwoScoreValid)
            {
                output = "Please enter a valid score value for team 2.";
            }
            else if (teamOneScore == 0 && teamTwoScore == 0)
            {
                output = "You did not enter a score for either team.";
            }
            else if (teamOneScore == teamTwoScore)
            {
                output = "The application does not allow ties.";
            }

            return output;
        }

        private void scoreButton_Click(object sender, RoutedEventArgs e)
        {
            var errorMessage = ValidateData();
            if (errorMessage.Length > 0)
            {
                MessageBox.Show($"Input error:\n{errorMessage}");
                return;
            }

            var model = (MatchupModel)matchupListBox.SelectedItem;
            if (model != null)
            {
                for (var i = 0; i < model.Entries.Count; ++i)
                {
                    if (i == 0)
                    {
                        if (model.Entries[0].TeamCompeting != null)
                        {
                            var isScoreValid = double.TryParse(teamOneScoreValue.Text, out var teamOneScore);
                            if (isScoreValid)
                            {
                                model.Entries[0].Score = teamOneScore;
                            }
                            else
                            {
                                MessageBox.Show("Please enter a valid score for team 1.");
                                return;
                            }
                        }
                    }

                    if (i == 1)
                    {
                        if (model.Entries[1].TeamCompeting != null)
                        {
                            var isScoreValid = double.TryParse(teamTwoScoreValue.Text, out var teamTwoScore);
                            if (isScoreValid)
                            {
                                model.Entries[1].Score = teamTwoScore;
                            }
                            else
                            {
                                MessageBox.Show("Please enter a valid score for team 2.");
                                return;
                            }
                        }
                    }
                }
            }

            try
            {
                TournamentLogic.UpdateTournamentResults(tournament);
            }
            catch (Exception exception) 
            {
                MessageBox.Show($"The application had the following error:\n{exception.Message}");
                return;
            }

            LoadMatchups((int)roundDropDown.SelectedItem);
        }
    }
}