using System.Windows;
using TrackerLibrary.Models;
using TrackerLibrary;
using MessageBox = System.Windows.MessageBox;

namespace TrackerUI
{
    /// <summary>
    /// Interaction logic for CreateTeamForm.xaml
    /// </summary>
    public partial class CreateTeamForm : Window
    {
        private List<PersonModel> availableTeamMembers = GlobalConfig.Connection.GetPerson_All();
        private List<PersonModel> selectedTeamMembers = new List<PersonModel>();
        private ITeamRequester callingForm;

        public CreateTeamForm(ITeamRequester caller)
        {
            InitializeComponent();
            callingForm = caller;
            WireUpLists();
        }

        private void WireUpLists()
        {
            // TODO - Sort availableTeamMembers and selectedTeamMembers by First Name.
            // DataSource is set to null for refresh purposes
            selectTeamMemberDropDown.ItemsSource = null;
            selectTeamMemberDropDown.ItemsSource = availableTeamMembers;
            selectTeamMemberDropDown.DisplayMemberPath = "FullName";

            // DataSource is set to null for refresh purposes
            teamMembersListBox.ItemsSource = null;
            teamMembersListBox.ItemsSource = selectedTeamMembers;
            teamMembersListBox.DisplayMemberPath = "FullName";
        }

        private void createMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                var newMember = new PersonModel
                {
                    FirstName = firstNameValue.Text,
                    LastName = lastNameValue.Text,
                    EmailAddress = emailAddressValue.Text,
                    CellphoneNumber = cellPhoneNumberValue.Text
                };

                // Creating a new member (Person) and Capturing the new id created for the newMember
                GlobalConfig.Connection.CreatePerson(newMember);

                selectedTeamMembers.Add(newMember);
                WireUpLists();

                // Clear 'Add New Member' form fields.
                firstNameValue.Text = "";
                lastNameValue.Text = "";
                emailAddressValue.Text = "";
                cellPhoneNumberValue.Text = "";
            }
            else
            {
                MessageBox.Show("Invalid fields");
            }
        }

        private bool ValidateForm()
        {
            if (firstNameValue.Text.Length == 0 || !firstNameValue.Text.All(char.IsLetter))
            {
                return false;
            }
            if (lastNameValue.Text.Length == 0 || !lastNameValue.Text.All(char.IsLetter))
            {
                return false;
            }
            if (emailAddressValue.Text.Length == 0)
            {
                return false;
            }
            if (cellPhoneNumberValue.Text.Length == 0)
            {
                return false;
            }

            return true;
        }

        private void addMemeberButton_Click(object sender, RoutedEventArgs e)
        {
            var person = (PersonModel)selectTeamMemberDropDown.SelectedItem;

            if (person == null) return;

            availableTeamMembers.Remove(person);
            selectedTeamMembers.Add(person);

            WireUpLists();
        }

        private void removeSelectedMemberButton_Click(object sender, RoutedEventArgs e)
        {
            var person = (PersonModel)teamMembersListBox.SelectedItem;

            if (person == null) return;

            selectedTeamMembers.Remove(person);
            availableTeamMembers.Add(person);

            WireUpLists();
        }

        private void createTeamButton_Click(object sender, RoutedEventArgs e)
        {
            var team = new TeamModel();

            team.TeamName = teamNameValue.Text;
            team.TeamMembers = selectedTeamMembers;

            GlobalConfig.Connection.CreateTeam(team);
            callingForm.TeamComplete(team);

            this.Close();
        }
    }
}
