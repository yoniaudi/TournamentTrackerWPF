using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string db = "Tournaments";

        // TODO - Make the CreatePrize method actually save to the database.
        /// <summary>
        /// Saves a new model in the database.
        /// </summary> 
        /// <param name="model">The model information.</param>
        /// <returns>The model information including the unique identifier.</returns>
        private void SaveTournament(IDbConnection connection, TournamentModel model)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TournamentName", model.TournamentName);
            parameters.Add("@EntryFee", model.EntryFee);
            parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spTournaments_Insert", parameters,
                commandType: CommandType.StoredProcedure);
            model.Id = parameters.Get<int>("@id");
        }

        private void SaveTournamentPrizes(IDbConnection connection, TournamentModel model)
        {
            foreach (var prize in model.Prizes)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TournamentId", model.Id);
                parameters.Add("@PrizeId", prize.Id);
                parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentPrizes_Insert", parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        private void SaveTournamentEntries(IDbConnection connection, TournamentModel model)
        {
            foreach (var team in model.EnteredTeams)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TournamentId", model.Id);
                parameters.Add("@TeamId", team.Id);
                parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentEntries_Insert", parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        private void SaveTournamentRounds(IDbConnection connection, TournamentModel model)
        {
            foreach (var round in model.Rounds)
            {
                foreach (var matchup in round)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TournamentId", model.Id);
                    parameters.Add("@MatchupRound", matchup.MatchupRound);
                    parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    connection.Execute("dbo.spMatchups_Insert", parameters, commandType: CommandType.StoredProcedure);

                    matchup.Id = parameters.Get<int>("@id");

                    foreach (var entry in matchup.Entries)
                    {
                        parameters = new DynamicParameters();
                        parameters.Add("@MatchupId", matchup.Id);
                        parameters.Add("@ParentMatchupId", entry.ParentMatchup?.Id);
                        parameters.Add("@TeamCompetingId", entry.TeamCompeting?.Id);
                        parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                        connection.Execute("dbo.spMatchupEntries_Insert", parameters,
                            commandType: CommandType.StoredProcedure);
                    }
                }
            }
        }

        public void CreatePerson(PersonModel model)
        {
            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FirstName", model.FirstName);
                parameters.Add("@LastName", model.LastName);
                parameters.Add("@EmailAddress", model.EmailAddress);
                parameters.Add("@CellPhoneNumber", model.CellphoneNumber);
                parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPeople_Insert", parameters, commandType: CommandType.StoredProcedure);
                model.Id = parameters.Get<int>("@id");
            }
        }

        public void CreatePrize(PrizeModel model)
        {
            using (IDbConnection connection =
                   new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@PlaceNumber", model.PlaceNumber);
                parameters.Add("@PlaceName", model.PlaceName);
                parameters.Add("@PrizeAmount", model.PrizeAmount);
                parameters.Add("@PrizePercentage", model.PrizePercentage);
                parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPrizes_Insert", parameters, commandType: CommandType.StoredProcedure);
                model.Id = parameters.Get<int>("@id");
            }
        }

        public void CreateTeam(TeamModel model)
        {
            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TeamName", model.TeamName);
                parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTeams_Insert", parameters, commandType: CommandType.StoredProcedure);
                model.Id = parameters.Get<int>("@id");

                foreach (var teamMember in model.TeamMembers)
                {
                    parameters = new DynamicParameters();
                    parameters.Add("@TeamId", model.Id);
                    parameters.Add("@PersonId", teamMember.Id);

                    connection.Execute("dbo.spTeamMembers_Insert", parameters, commandType: CommandType.StoredProcedure);
                }
            }
        }

        public void CreateTournament(TournamentModel model)
        {
            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                SaveTournament(connection, model);
                SaveTournamentPrizes(connection, model);
                SaveTournamentEntries(connection, model);
                SaveTournamentRounds(connection, model);

                TournamentLogic.UpdateTournamentResults(model);
            }
        }

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;

            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }

            return output;
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;

            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TeamModel>("dbo.spTeam_GetAll").ToList();

                foreach (var team in output)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TeamId", team.Id);
                    team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }

            return output;
        }

        public List<TournamentModel> GetTournament_All()
        {
            List<TournamentModel> output;

            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TournamentModel>("dbo.spTournaments_GetAll").ToList();

                foreach (var tournament in output)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TournamentId", tournament.Id);
                    // Populate Prizes
                    tournament.Prizes = connection.Query<PrizeModel>("dbo.spPrizes_GetByTournament", parameters,
                        commandType: CommandType.StoredProcedure).ToList();

                    // Populate Teams
                    tournament.EnteredTeams = connection.Query<TeamModel>("dbo.spTeam_GetByTournament", parameters,
                        commandType: CommandType.StoredProcedure).ToList();

                    foreach (var team in tournament.EnteredTeams)
                    {
                        parameters = new DynamicParameters();
                        parameters.Add("@TeamId", team.Id);

                        // Populate People
                        team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", parameters,
                            commandType: CommandType.StoredProcedure).ToList();
                    }

                    parameters = new DynamicParameters();
                    parameters.Add("@TournamentId", tournament.Id);

                    // Populate Rounds
                    var matchups = connection.Query<MatchupModel>("dbo.spMatchups_GetByTournament", parameters,
                        commandType: CommandType.StoredProcedure).ToList();

                    foreach (var matchup in matchups)
                    {
                        parameters = new DynamicParameters();
                        parameters.Add("@MatchupId", matchup.Id);

                        // Populate rounds
                        matchup.Entries = connection.Query<MatchupEntryModel>("dbo.spMatchupEntries_GetByMatchup",
                            parameters, commandType: CommandType.StoredProcedure).ToList();

                        var teams = GetTeam_All();

                        if (matchup.WinnerId > 0)
                        {
                            matchup.Winner = teams.First(x => x.Id == matchup.WinnerId);
                        }

                        foreach (var entry in matchup.Entries)
                        {
                            if (entry.TeamCompetingId > 0)
                            {
                                entry.TeamCompeting = teams.First(x => x.Id == entry.TeamCompetingId);
                            }

                            if (entry.ParentMatchupId > 0)
                            {
                                entry.ParentMatchup = matchups.First(x => x.Id == entry.ParentMatchupId);
                            }
                        }
                    }

                    // List<List<MatchupModel>>
                    var currentRow = new List<MatchupModel>();
                    var currentRound = 1;

                    foreach (var matchup in matchups)
                    {
                        if (matchup.MatchupRound > currentRound)
                        {
                            tournament.Rounds.Add(currentRow);
                            currentRow = new List<MatchupModel>();
                            currentRound++;
                        }
                        currentRow.Add(matchup);
                    }
                    tournament.Rounds.Add(currentRow);
                }
            }
            return output;
        }

        public void UpdateMatchup(MatchupModel model)
        {
            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var parameters = new DynamicParameters();

                if (model.Winner != null)
                {
                    parameters.Add("@id", model.Id);
                    parameters.Add("@WinnerId", model.Winner.Id);

                    connection.Execute("dbo.spMatchups_Update", parameters, commandType: CommandType.StoredProcedure);
                }

                foreach (var entry in model.Entries)
                {
                    if (entry.TeamCompeting != null)
                    {
                        parameters = new DynamicParameters();
                        parameters.Add("@id", entry.Id);
                        parameters.Add("@TeamCompetingId", entry.TeamCompeting.Id);
                        parameters.Add("@Score", entry.Score);

                        connection.Execute("dbo.spMatchupEntries_Update", parameters, commandType: CommandType.StoredProcedure);
                    }
                }
            }
        }

        public void CompleteTournament(TournamentModel model)
        {
            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", model.Id);

                connection.Execute("dbo.spTournaments_Complete", parameters, commandType: CommandType.StoredProcedure);
            }
        }
    }
}