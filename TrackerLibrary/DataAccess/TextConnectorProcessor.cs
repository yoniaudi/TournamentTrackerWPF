using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName)
        {
            return $"{ ConfigurationManager.AppSettings["filepath"] }\\{ fileName }";
        }

        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file)) return new List<string>();

            return File.ReadAllLines(file).ToList();
        }

        public static List<TournamentModel> ConvertFileToTournamentModels(this List<string> lines)
        {
            var output = new List<TournamentModel>();
            var teams = GlobalConfig.TeamsFileName.FullFilePath().LoadFile().ConvertFileToTeamModels();
            var prizes = GlobalConfig.PrizesFileName.FullFilePath().LoadFile().ConvertFileToPrizeModels();
            var matchups = GlobalConfig.MatchupFileName.FullFilePath().LoadFile().ConvertFileToMatchupModels();

            foreach (var line in lines)
            {
                var tournament = new TournamentModel();
                var column = line.Split(',');

                tournament.Id = int.Parse(column[0]);
                tournament.TournamentName = column[1];
                tournament.EntryFee = decimal.Parse(column[2]);

                var teamIds = column[3].Split('|');

                foreach (var id in teamIds)
                {
                    tournament.EnteredTeams.Add(teams.First(x => x.Id == int.Parse(id)));
                }

                var prizeIds = column[4].Split('|');
                
                foreach (var id in prizeIds)
                {
                    if (id != "")
                    {
                        tournament.Prizes.Add(prizes.First(x => x.Id == int.Parse(id)));
                    }
                }

                var rounds = column[5].Split('|');

                foreach (var round in rounds)
                {
                    var matchupIds = round.Split('^');
                    var newTournamentRound = new List<MatchupModel>();

                    foreach (var id in matchupIds)
                    {
                        newTournamentRound.Add(matchups.FirstOrDefault(x => x.Id == int.Parse(id)));
                    }

                    tournament.Rounds.Add(newTournamentRound);
                }

                output.Add(tournament);
            }

            return output;
        }

        public static void SaveToTournamentsFile(this List<TournamentModel> tournaments)
        {
            var lines = new List<string>();

            foreach (var tournament in tournaments)
            {
                lines.Add($"{tournament.Id},{tournament.TournamentName},{tournament.EntryFee}" +
                          $",{ConvertListToString(tournament.EnteredTeams, team => team.Id)}" +
                          $",{ConvertListToString(tournament.Prizes, prize => prize.Id)}" +
                          $",{ConvertRoundListToString(tournament.Rounds)}");
            }

            File.WriteAllLines(GlobalConfig.TournamentsFileName.FullFilePath(), lines);
        }

        public static List<TeamModel> ConvertFileToTeamModels(this List<string> lines)
        {
            var output = new List<TeamModel>();
            var people = GlobalConfig.PeopleFileName.FullFilePath().LoadFile().ConvertFileToPersonModels();

            foreach (var line in lines)
            {
                var team = new TeamModel();
                var column = line.Split(',');

                team.Id = int.Parse(column[0]);
                team.TeamName = column[1];

                var teamMembersId = column[2].Split('|');
                
                foreach (var id in teamMembersId)
                {
                    team.TeamMembers.Add(people.First(x => x.Id == int.Parse(id)));
                }

                output.Add(team);
            }
            
            return output;
        }

        public static void SaveToTeamsFile(this List<TeamModel> teams)
        {
            var lines = new List<string>();

            foreach (var team in teams)
            {
                lines.Add($"{team.Id},{team.TeamName},{ConvertListToString(team.TeamMembers, person => person.Id)}");
            }

            File.WriteAllLines(GlobalConfig.TeamsFileName.FullFilePath(), lines);
        }

        public static List<MatchupModel> ConvertFileToMatchupModels(this List<string> lines)
        {
            var output = new List<MatchupModel>();

            foreach (var line in lines)
            {
                string[] column = line.Split(',');

                output.Add(new MatchupModel()
                {
                    Id = int.Parse(column[0]),
                    MatchupRound = int.Parse(column[1]),
                    Winner = int.TryParse(column[2], out var winnerId) ? GetTeamModelById(winnerId) : null,
                    Entries = ConvertStringToMatchupEntryModels(column[3])
                });
            }

            return output;
        }

        public static void  SaveToMatchupsFile(this MatchupModel model)
        {
            var matchups = GlobalConfig.MatchupFileName.FullFilePath().LoadFile().ConvertFileToMatchupModels();

            var currentId = 1;
            
            if (matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;
            matchups.Add(model);

            foreach (var entry in model.Entries)
            {
                entry.SaveToEntriesFile();
            }

            var lines = new List<string>();

            foreach (var matchup in matchups)
            {
                lines.Add($"{matchup.Id},{matchup.MatchupRound},{matchup.Winner?.Id},{ConvertListToString(matchup.Entries, entry => entry.Id)}");
            }

            File.WriteAllLines(GlobalConfig.MatchupFileName.FullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel model)
        {
            var matchups = GlobalConfig.MatchupFileName.FullFilePath().LoadFile().ConvertFileToMatchupModels();
            var oldMatchup = new MatchupModel();

            foreach (var matchup in matchups)
            {
                if (matchup.Id == model.Id)
                {
                    oldMatchup = matchup;
                    break;
                }
            }

            matchups.Remove(oldMatchup);
            matchups.Add(model);

            foreach (var entry in model.Entries)
            {
                entry.UpdateEntriesToFile();
            }

            var lines = new List<string>();

            foreach (var matchup in matchups)
            {
                lines.Add($"{matchup.Id},{matchup.MatchupRound},{matchup.Winner?.Id},{ConvertListToString(matchup.Entries, entry => entry.Id)}");
            }

            File.WriteAllLines(GlobalConfig.MatchupFileName.FullFilePath(), lines);
        }

        public static List<MatchupEntryModel> ConvertFileToMatchupEntryModels(this List<string> lines)
        {
            var output = new List<MatchupEntryModel>();

            foreach (var line in lines)
            {
                string[] column = line.Split(',');

                output.Add(new MatchupEntryModel()
                {
                    Id = int.Parse(column[0]),
                    TeamCompeting = int.TryParse(column[1], out var teamCompetingId) ? GetTeamModelById(teamCompetingId) : null,
                    Score = double.Parse(column[2]),
                    ParentMatchup = int.TryParse(column[3], out var parentId) ? GetMatchupModelById(parentId) : null,
                });
            }

            return output;
        }

        public static void SaveToEntriesFile(this MatchupEntryModel model)
        {
            var entries = GlobalConfig.MatchupEntryFileName.FullFilePath().LoadFile()
                .ConvertFileToMatchupEntryModels();

            var currentId = 1;

            if (entries.Count > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;
            entries.Add(model);

            var lines = new List<string>();

            foreach (var entry in entries)
            {
                lines.Add($"{entry.Id},{entry.TeamCompeting?.Id},{entry.Score},{entry.ParentMatchup?.Id}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFileName.FullFilePath(), lines);
        }

        public static void UpdateEntriesToFile(this MatchupEntryModel model)
        {
            var entries = GlobalConfig.MatchupEntryFileName.FullFilePath().LoadFile()
                .ConvertFileToMatchupEntryModels();
            var oldEntry = new MatchupEntryModel();

            foreach (var entry in entries)
            {
                if (entry.Id == model.Id)
                {
                    oldEntry = entry;
                    break;
                }
            }

            entries.Remove(oldEntry);
            entries.Add(model);

            var lines = new List<string>();

            foreach (var entry in entries)
            {
                lines.Add($"{entry.Id},{entry.TeamCompeting?.Id},{entry.Score},{entry.ParentMatchup?.Id}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFileName.FullFilePath(), lines);
        }

        public static List<PersonModel> ConvertFileToPersonModels(this List<string> lines)
        {
            var output = new List<PersonModel>();

            foreach (var line in lines)
            {
                string[] column = line.Split(',');

                var personModel = new PersonModel
                {
                    Id = int.Parse(column[0]),
                    FirstName = column[1],
                    LastName = column[2],
                    EmailAddress = column[3],
                    CellphoneNumber = column[4]
                };

                output.Add(personModel);
            }

            return output;
        }

        public static void SaveToPeopleFile(this List<PersonModel> people)
        {
            var lines = new List<string>();

            foreach (var person in people)
            {
                lines.Add($"{ person.Id },{ person.FirstName },{ person.LastName },{ person.EmailAddress },{ person.CellphoneNumber }");
            }

            File.WriteAllLines(GlobalConfig.PeopleFileName.FullFilePath(), lines);
        }
        
        public static List<PrizeModel> ConvertFileToPrizeModels(this List<string> lines)
        {
            var output = new List<PrizeModel>();

            foreach (var line in lines)
            {
                string[] column = line.Split(',');
                
                var prizeModel = new PrizeModel
                {
                    Id = int.Parse(column[0]),
                    PlaceNumber = int.Parse(column[1]),
                    PlaceName = column[2],
                    PrizeAmount = decimal.Parse(column[3]),
                    PrizePercentage = double.Parse(column[4])
                };

                output.Add(prizeModel);
            }

            return output;
        }

        public static void SaveToPrizesFile(this List<PrizeModel> prizes)
        {
            var lines = new List<string>();

            foreach (var prize in prizes)
            {
                lines.Add($"{ prize.Id },{ prize.PlaceNumber },{ prize.PlaceName },{ prize.PrizeAmount },{ prize.PrizePercentage }");
            }

            File.WriteAllLines(GlobalConfig.PrizesFileName.FullFilePath(), lines);
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            //loop through each round
            //loop through each matchup
            //get the id from the new matchup and save the record
            //loop through each entry, get the id and save it

            foreach (var round in model.Rounds)
            {
                foreach (var matchup in round)
                {
                    //load all of the matchups from file
                    //get the top id and add one
                    //store the id
                    // save the matchup record
                    matchup.SaveToMatchupsFile();
                }
            }
        }

        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            var ids = input.Split('|');
            var entries = GlobalConfig.MatchupEntryFileName.FullFilePath().LoadFile();
            var matchingEntries = new List<string>();

            foreach (var id in ids)
            {
                foreach (var entry in entries)
                {
                    var column = entry.Split(',');
                    
                    if (column[0] == id)
                    {
                        matchingEntries.Add(entry);
                        //can add break
                    }
                }
            }

            return matchingEntries.ConvertFileToMatchupEntryModels();
        }

        /// <summary>
        /// Search in the TeamModels.csv file for a Team by it's id.
        /// </summary>
        /// <param name="id">The id of the Team we are looking for.</param>
        /// <returns></returns>
        private static TeamModel GetTeamModelById(int id)
        {
            var teams = GlobalConfig.TeamsFileName.FullFilePath().LoadFile();

            foreach (var team in teams)
            {
                var column = team.Split(',');

                if (column[0] == id.ToString())
                {
                    return new List<string> { team }.ConvertFileToTeamModels().First();
                }
            }

            return null;
        }

        /// <summary>
        /// Search in the MatchupModels.csv file for a Matchup by it's id.
        /// </summary>
        /// <param name="id">The id of the Matchup we are looking for.</param>
        /// <returns></returns>
        private static MatchupModel GetMatchupModelById(int id)
        {
            var matchups = GlobalConfig.MatchupFileName.FullFilePath().LoadFile();

            foreach (var matchup in matchups)
            {
                var column = matchup.Split(',');

                if (column[0] == id.ToString())
                {
                    return new List<string> { matchup }.ConvertFileToMatchupModels().First();
                }
            }

            return null;
        }

        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {
            if (rounds.Count == 0) return "";

            var output = "";

            foreach (var round in rounds)
            {
                output += $"{ConvertMatchupListToString(round)}|";
            }

            return output.Substring(0, output.Length - 1);
        }

        private static string ConvertMatchupListToString(List<MatchupModel> round)
        {
            if (round.Count == 0) return "";

            var output = "";

            foreach (var matchup in round)
            {
                output += $"{matchup?.Id}^";
            }

            return output.Substring(0, output.Length - 1);
        }

        private static string ConvertListToString<T>(List<T> items, Func<T, int> getId)
        {
            if (items.Count == 0) return "";

            var output = "";

            // getId function call example:
            // string result = ConvertListToString(items, item => item.Id)
        
            foreach (var item in items)
            {
                output += $"{getId(item)}|";
            }

            return output.Substring(0, output.Length - 1);
        }
    }
}   