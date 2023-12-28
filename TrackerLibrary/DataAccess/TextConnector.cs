using System.Collections.Generic;
using System.Linq;
using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;

namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {
        /// <summary>
        /// Saves a new model in the database.
        /// </summary>
        /// <param name="model">The model information.</param>
        /// <returns>The model information including the unique identifier.</returns>
        public void CreateTournament(TournamentModel model)
        {
            var tournaments = GlobalConfig.TournamentsFileName.FullFilePath()
                .LoadFile()
                .ConvertFileToTournamentModels();

            var currentId = 1;

            if (tournaments.Count > 0)
            {
                currentId = tournaments.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;
            model.SaveRoundsToFile();
            tournaments.Add(model);
            tournaments.SaveToTournamentsFile();

            TournamentLogic.UpdateTournamentResults(model);
        } 

        public List<TournamentModel> GetTournament_All()
        {
            return GlobalConfig.TournamentsFileName.FullFilePath().LoadFile().ConvertFileToTournamentModels();
        }

        public void CreateTeam(TeamModel model)
        {
            var teams = GlobalConfig.TeamsFileName.FullFilePath().LoadFile().ConvertFileToTeamModels();

            var currentId = 1;

            if (teams.Count > 0)
            {
                currentId = teams.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;
            teams.Add(model);
            teams.SaveToTeamsFile();
        }

        public void CreatePerson(PersonModel model)
        {
            var people = GlobalConfig.PeopleFileName.FullFilePath().LoadFile().ConvertFileToPersonModels();

            var currentId = 1;

            if (people.Count > 0)
            {
                currentId = people.OrderByDescending(x => x.Id).First().Id + 1;
            }
            
            model.Id = currentId;
            people.Add(model);
            people.SaveToPeopleFile();
        }

        public void CreatePrize(PrizeModel model)
        {
            var prizes = GlobalConfig.PrizesFileName.FullFilePath().LoadFile().ConvertFileToPrizeModels();

            var currentId = 1;

            if (prizes.Count > 0)
            { 
                currentId = prizes.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;
            prizes.Add(model);
            prizes.SaveToPrizesFile();
        }

        public List<TeamModel> GetTeam_All()
        {
            return GlobalConfig.TeamsFileName.FullFilePath().LoadFile().ConvertFileToTeamModels();
        }

        public List<PersonModel> GetPerson_All()
        {
            return GlobalConfig.PeopleFileName.FullFilePath().LoadFile().ConvertFileToPersonModels();
        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchupToFile();
        }

        public void CompleteTournament(TournamentModel model)
        {
            /*var tournaments = GlobalConfig.TournamentsFileName.FullFilePath()
                .LoadFile()
                .ConvertFileToTournamentModels();

            tournaments.Remove(model);
            tournaments.SaveToTournamentsFile();

            TournamentLogic.UpdateTournamentResults(model);*/
        }
    }
}
