using Azure.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel model)
        {
            var randomizedTeams = RandomizeTeamOrder(model.EnteredTeams);
            var numberOfRounds = FindNumberOfRounds(model.EnteredTeams.Count);
            var byes = NumberOfByes(numberOfRounds, model.EnteredTeams.Count);

            model.Rounds.Add(CreateFirstRound(randomizedTeams, byes));
            CreateOtherRounds(model, numberOfRounds);
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            var startingRoundNumber = model.CheckCurrentRound();
            var unscoredMatchups = new List<MatchupModel>();

            foreach (var round in model.Rounds)
            {
                foreach (var matchup in round)
                {
                    if (matchup.Winner == null &&
                        (matchup.Entries.Any(x => x.Score != 0) || matchup.Entries.Count == 1))
                    {
                        unscoredMatchups.Add(matchup);
                    }
                }
            }

            MarkWinnerInMatchups(unscoredMatchups);
            AdvanceWinners(unscoredMatchups, model);
            unscoredMatchups.ForEach(GlobalConfig.Connection.UpdateMatchup);

            var endingRoundNumber = model.CheckCurrentRound();

            /*if (endingRoundNumber > startingRoundNumber)
            {
                model.AlertUsersToNewRound();
            }*/
        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            var currentRoundNumber = model.CheckCurrentRound();
            var currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

            foreach (var matchup in currentRound)
            {
                foreach (var entry in matchup.Entries)
                {
                    foreach (var person in entry.TeamCompeting.TeamMembers)
                    {
                        AlertPersonToNewRound(person, matchup.Entries.Where(x => x.TeamCompeting != entry.TeamCompeting).FirstOrDefault());
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel person, MatchupEntryModel? competitor)
        {
            if (person.EmailAddress.Length == 0) return;

            var subject = "";
            var body = new StringBuilder();

            if (competitor != null)
            {
                subject = $"You have a new matchup with {competitor.TeamCompeting.TeamName}";

                body.AppendLine("<h1>You have a new matchup.</h1>");
                body.Append("<strong>Competitor: </strong>");
                body.Append(competitor.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine();
                body.AppendLine("Have a great time!");
                body.AppendLine("~Tournament Tracker App.");
            }
            else
            {
                subject = "You have a bye week this round.";

                body.AppendLine("Enjoy your round off!");
                body.AppendLine("~Tournament Tracker App.");
            }

            EmailLogic.SendEmail(person.EmailAddress, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            var output = 1;

            foreach (var round in model.Rounds)
            {
                if (round.All(x => x.Winner != null))
                {
                    ++output;
                }
                else
                {
                    return output;
                }
            }

            CompleteTournament(model);

            return output - 1;
        }

        private static void CompleteTournament(TournamentModel model)
        {
            GlobalConfig.Connection.CompleteTournament(model);
            var winners = model.Rounds.Last().First().Winner;
            var runnerUp = model.Rounds.Last().First().Entries.Where(x => x.TeamCompeting != winners).First().TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerUpPrize = 0;

            if (model.Prizes.Count > 0)
            {
                var totalIncome = model.EnteredTeams.Count * model.EntryFee;
                var firstPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                var secondPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();

                if (firstPlacePrize != null)
                {
                    winnerPrize = firstPlacePrize.CalculatePrizePayout(totalIncome);
                }

                if (secondPlacePrize != null)
                {
                    runnerUpPrize = secondPlacePrize.CalculatePrizePayout(totalIncome);
                }
            }

            // Can send an email to all tournament's participants declaring the tournament winner.

            // Complete tournament
            model.CompleteTournament();
        }

        private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output = 0;

            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output = Decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercentage / 100));
            }

            return output;
        }

        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            foreach (var model in models)
            {
                foreach (var round in tournament.Rounds)
                {
                    foreach (var matchup in round)
                    {
                        foreach (var entry in matchup.Entries)
                        {
                            if (entry.ParentMatchup != null)
                            {
                                if (entry.ParentMatchup.Id == model.Id)
                                {
                                    entry.TeamCompeting = model.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(matchup);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            var greaterWins = ConfigurationManager.AppSettings["greaterWins"];
            var errorMessage = "Application does not handle ties.";

            foreach (var model in models)
            {
                if (model.Entries.Count == 1)
                {
                    model.Winner = model.Entries[0].TeamCompeting;
                    continue;
                }

                if (greaterWins == "1")
                {
                    if (model.Entries[0].Score > model.Entries[1].Score)
                    {
                        model.Winner = model.Entries[0].TeamCompeting;
                    }
                    else if (model.Entries[0].Score < model.Entries[1].Score)
                    {
                        model.Winner = model.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception(errorMessage);
                    }
                }
                else
                {
                    if (model.Entries[0].Score < model.Entries[1].Score)
                    {
                        model.Winner = model.Entries[0].TeamCompeting;
                    }
                    else if (model.Entries[0].Score > model.Entries[1].Score)
                    {
                        model.Winner = model.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception(errorMessage);
                    }
                }
            }
        }

        private static void CreateOtherRounds(TournamentModel model, int numberOfRounds)
        {
            var round = 2;
            var previousRound = model.Rounds[0];
            var currentRound = new List<MatchupModel>();
            var currentMatchup = new MatchupModel();

            while (round <= numberOfRounds)
            {
                foreach (var matchup in previousRound)
                {
                    currentMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = matchup });

                    if (currentMatchup.Entries.Count > 1)
                    {
                        currentMatchup.MatchupRound = round;
                        currentRound.Add(currentMatchup);
                        currentMatchup = new MatchupModel();
                    }
                }

                model.Rounds.Add(currentRound);
                previousRound = currentRound;
                currentRound = new List<MatchupModel>();
                ++round;
            }
        }

        private static List<MatchupModel> CreateFirstRound(List<TeamModel> teams, int byes)
        {
            var output = new List<MatchupModel>();
            var matchup = new MatchupModel();

            foreach (var team in teams)
            {
                matchup.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

                if (byes > 0 || matchup.Entries.Count > 1)
                {
                    matchup.MatchupRound = 1;
                    output.Add(matchup);
                    matchup = new MatchupModel();

                    if (byes > 0)
                    {
                        byes--;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Returns the number of byes in a tournament.
        /// Byes is a state in which a participant in the competition automatically rises.
        /// </summary>
        /// <param name="numberOfRounds">The number of rounds in the tournament.</param>
        /// <param name="numberOfTeams">The number of teams participating in the tournament.</param>
        /// <returns></returns>
        private static int NumberOfByes(int numberOfRounds, int numberOfTeams)
        {
            var totalTeams = 1;

            for (var i = 1; i <= numberOfRounds; ++i)
            {
                totalTeams *= 2;
            }

            return totalTeams - numberOfTeams;
        }

        private static int FindNumberOfRounds(int numberOfTeams)
        {
            var numberOfRounds = 1;
            var estimatedMaximumRounds = 2;

            while (estimatedMaximumRounds < numberOfTeams)
            {
                numberOfRounds++;
                estimatedMaximumRounds *= 2;
            }

            return numberOfRounds;
        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            return teams.OrderBy(x => Guid.NewGuid()).ToList();
        }
    }
}